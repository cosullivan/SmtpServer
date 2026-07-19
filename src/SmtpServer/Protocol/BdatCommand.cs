using System;
using System.Buffers;
using System.IO;
using System.IO.Pipelines;
using System.Threading;
using System.Threading.Tasks;
using SmtpServer.ComponentModel;
using SmtpServer.IO;
using SmtpServer.Storage;

namespace SmtpServer.Protocol
{
    /// <summary>
    /// Bdat Command
    /// </summary>
    public sealed class BdatCommand : SmtpCommand
    {
        internal const string LastChunkKey = "SmtpServer:Bdat:LastChunk";
        const string TransactionKey = "SmtpServer:Bdat:Transaction";

        /// <summary>
        /// Smtp Bdat Command
        /// </summary>
        public const string Command = "BDAT";

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="size">The chunk size.</param>
        /// <param name="isLast">Whether this is the last chunk.</param>
        public BdatCommand(long size, bool isLast) : base(Command)
        {
            Size = size;
            IsLast = isLast;
        }

        /// <inheritdoc />
        internal override async Task<bool> ExecuteAsync(SmtpSessionContext context, CancellationToken cancellationToken)
        {
            if (context.ServerOptions.Extensions.ChunkingEnabled == false)
            {
                await context.Pipe.Output.WriteReplyAsync(new SmtpResponse(SmtpReplyCode.CommandNotImplemented, "CHUNKING is not enabled"), cancellationToken).ConfigureAwait(false);
                return false;
            }

            context.Properties[LastChunkKey] = IsLast;

            if (context.Transaction.To.Count == 0)
            {
                await context.Pipe.Output.WriteReplyAsync(SmtpResponse.NoValidRecipientsGiven, cancellationToken).ConfigureAwait(false);
                return false;
            }

            var transaction = GetOrCreateTransaction(context);

            try
            {
                if (WouldExceedMessageSize(context, transaction.Length + Size))
                {
                    await ReadChunkAsync(context.Pipe.Input, Size, null, cancellationToken).ConfigureAwait(false);
                    await AbortAsync(context).ConfigureAwait(false);
                    throw new SmtpResponseException(SmtpResponse.MaxMessageSizeExceeded, true);
                }

                await transaction.AppendAsync(context.Pipe.Input, Size, cancellationToken).ConfigureAwait(false);

                if (IsLast == false)
                {
                    await context.Pipe.Output.WriteReplyAsync(SmtpResponse.Ok, cancellationToken).ConfigureAwait(false);
                    return true;
                }

                var response = await CompleteTransactionAsync(context, transaction, cancellationToken).ConfigureAwait(false);
                await context.Pipe.Output.WriteReplyAsync(response, cancellationToken).ConfigureAwait(false);
                return true;
            }
            catch (SmtpResponseException)
            {
                throw;
            }
            catch (Exception)
            {
                await AbortAsync(context).ConfigureAwait(false);
                await context.Pipe.Output.WriteReplyAsync(new SmtpResponse(SmtpReplyCode.TransactionFailed), cancellationToken).ConfigureAwait(false);
                return false;
            }
        }

        static bool WouldExceedMessageSize(SmtpSessionContext context, long length)
        {
            return context.ServerOptions.MaxMessageSizeOptions.Handling == MaxMessageSizeHandling.Strict
                && length > context.ServerOptions.MaxMessageSizeOptions.Length;
        }

        static BdatTransaction GetOrCreateTransaction(SmtpSessionContext context)
        {
            if (context.Properties.TryGetValue(TransactionKey, out var value) && value is BdatTransaction transaction)
            {
                return transaction;
            }

            var messageStore = context.ServiceProvider.GetService<IMessageStoreFactory, IMessageStore>(context, MessageStore.Default);
            transaction = new BdatTransaction(context, messageStore);
            context.Properties[TransactionKey] = transaction;

            return transaction;
        }

        static async Task<SmtpResponse> CompleteTransactionAsync(SmtpSessionContext context, BdatTransaction transaction, CancellationToken cancellationToken)
        {
            try
            {
                return await transaction.CompleteAsync(cancellationToken).ConfigureAwait(false);
            }
            finally
            {
                context.Properties.Remove(TransactionKey);
            }
        }

        internal static async Task AbortAsync(SmtpSessionContext context)
        {
            context.Properties.Remove(LastChunkKey);

            if (context.Properties.TryGetValue(TransactionKey, out var value) && value is BdatTransaction transaction)
            {
                context.Properties.Remove(TransactionKey);
                await transaction.AbortAsync().ConfigureAwait(false);
            }
        }

        static async Task ReadChunkAsync(PipeReader reader, long size, Func<ReadOnlySequence<byte>, ValueTask> writeAsync, CancellationToken cancellationToken)
        {
            var remaining = size;

            while (remaining > 0)
            {
                var result = await reader.ReadAsync(cancellationToken).ConfigureAwait(false);
                var buffer = result.Buffer;

                if (buffer.IsEmpty && result.IsCompleted)
                {
                    throw new SmtpResponseException(new SmtpResponse(SmtpReplyCode.TransactionFailed), true);
                }

                var length = Math.Min(buffer.Length, remaining);
                var chunk = buffer.Slice(0, length);

                if (writeAsync != null)
                {
                    await writeAsync(chunk).ConfigureAwait(false);
                }

                remaining -= length;
                reader.AdvanceTo(buffer.GetPosition(length));
            }
        }

        static void Write(PipeWriter writer, ReadOnlySequence<byte> buffer)
        {
            var position = buffer.GetPosition(0);

            while (buffer.TryGet(ref position, out var memory))
            {
                if (memory.Length == 0)
                {
                    continue;
                }

                var span = writer.GetSpan(memory.Length);
                memory.Span.CopyTo(span);
                writer.Advance(memory.Length);
            }
        }

        /// <summary>
        /// Gets the chunk size.
        /// </summary>
        public long Size { get; }

        /// <summary>
        /// Gets a value indicating whether this is the last chunk.
        /// </summary>
        public bool IsLast { get; }

        sealed class BdatTransaction
        {
            readonly SmtpSessionContext _context;
            readonly IMessageStore _messageStore;
            readonly IStreamingMessageStore _streamingMessageStore;
            readonly MemoryStream _buffer;
            readonly Pipe _pipe;
            readonly Task<SmtpResponse> _saveTask;

            public BdatTransaction(SmtpSessionContext context, IMessageStore messageStore)
            {
                _context = context;
                _messageStore = messageStore;
                _streamingMessageStore = messageStore as IStreamingMessageStore;

                if (_streamingMessageStore == null)
                {
                    _buffer = new MemoryStream();
                }
                else
                {
                    _pipe = new Pipe();
                    _saveTask = _streamingMessageStore.SaveAsync(_context, _context.Transaction, _pipe.Reader, CancellationToken.None);
                }
            }

            public long Length { get; private set; }

            public async Task AppendAsync(PipeReader reader, long size, CancellationToken cancellationToken)
            {
                if (_streamingMessageStore == null)
                {
                    await ReadChunkAsync(
                        reader,
                        size,
                        chunk =>
                        {
                            foreach (var segment in chunk)
                            {
                                _buffer.Write(segment.Span);
                            }

                            return default;
                        },
                        cancellationToken).ConfigureAwait(false);
                }
                else
                {
                    await ReadChunkAsync(
                        reader,
                        size,
                        async chunk =>
                        {
                            Write(_pipe.Writer, chunk);
                            await _pipe.Writer.FlushAsync(cancellationToken).ConfigureAwait(false);
                        },
                        cancellationToken).ConfigureAwait(false);
                }

                Length += size;
            }

            public async Task<SmtpResponse> CompleteAsync(CancellationToken cancellationToken)
            {
                if (_streamingMessageStore == null)
                {
                    var buffer = CreateBufferedSequence(_buffer);
                    return await _messageStore.SaveAsync(_context, _context.Transaction, buffer, cancellationToken).ConfigureAwait(false);
                }

                await _pipe.Writer.CompleteAsync().ConfigureAwait(false);
                try
                {
                    return await _saveTask.ConfigureAwait(false);
                }
                finally
                {
                    await _pipe.Reader.CompleteAsync().ConfigureAwait(false);
                }
            }

            public async Task AbortAsync()
            {
                if (_streamingMessageStore == null)
                {
                    _buffer.Dispose();
                    return;
                }

                await _pipe.Writer.CompleteAsync().ConfigureAwait(false);

                try
                {
                    await _saveTask.ConfigureAwait(false);
                }
                catch
                {
                    // The caller is already handling the BDAT failure path.
                }
                finally
                {
                    await _pipe.Reader.CompleteAsync().ConfigureAwait(false);
                }
            }

            static ReadOnlySequence<byte> CreateBufferedSequence(MemoryStream buffer)
            {
                if (buffer.TryGetBuffer(out var segment) == false)
                {
                    return new ReadOnlySequence<byte>(buffer.ToArray());
                }

                return new ReadOnlySequence<byte>(segment.Array, segment.Offset, segment.Count);
            }
        }
    }
}

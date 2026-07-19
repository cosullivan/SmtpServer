using System;
using System.Buffers;
using System.IO.Pipelines;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SmtpServer.Protocol;
using SmtpServer.Text;

namespace SmtpServer.IO
{
    internal static class PipeReaderExtensions
    {
        // ReSharper disable once InconsistentNaming
        static readonly byte[] CRLF = { 13, 10 };
        static readonly byte[] DotBlock = { 13, 10, 46, 13, 10 };
        static readonly byte[] DotBlockStuffing = { 13, 10, 46, 46 };
        const int DotBlockTailLength = 4;

        /// <summary>
        /// Read from the reader until the sequence is found.
        /// </summary>
        /// <param name="reader">The reader to read from.</param>
        /// <param name="sequence">The sequence to find to terminate the read operation.</param>
        /// <param name="func">The callback to execute to process the buffer.</param>
        /// <param name="maxMessageSizeOptions">Handling of MaxMessageSize.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The value that was read from the buffer.</returns>
        static async ValueTask ReadUntilAsync(PipeReader reader, byte[] sequence, Func<ReadOnlySequence<byte>, Task> func, IMaxMessageSizeOptions maxMessageSizeOptions, CancellationToken cancellationToken)
        {
            if (reader == null)
            {
                throw new ArgumentNullException(nameof(reader));
            }
            
            var read = await reader.ReadAsync(cancellationToken);
            var head = read.Buffer.Start;

            while (read.IsCanceled == false && read.IsCompleted == false && read.Buffer.IsEmpty == false)
            {
                if (maxMessageSizeOptions.Handling == MaxMessageSizeHandling.Strict && read.Buffer.Length > maxMessageSizeOptions.Length)
                {
                    throw new SmtpResponseException(SmtpResponse.MaxMessageSizeExceeded, true);
                }

                if (read.Buffer.TryFind(sequence, ref head, out var tail))
                {
                    try
                    {
                        await func(read.Buffer.Slice(read.Buffer.Start, head));
                    }
                    finally
                    {
                        reader.AdvanceTo(tail);
                    }

                    return;
                }

                reader.AdvanceTo(read.Buffer.Start, read.Buffer.End);
                
                read = await reader.ReadAsync(cancellationToken);
            }
        }

        /// <summary>
        /// Read from the reader until the sequence is found.
        /// </summary>
        /// <param name="reader">The reader to read from.</param>
        /// <param name="sequence">The sequence to find to terminate the read operation.</param>
        /// <param name="func">The callback to execute to process the buffer.</param>
        /// <param name="maxLength">The maximum length to read before the terminator.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The value that was read from the buffer.</returns>
        static async ValueTask ReadUntilAsync(PipeReader reader, byte[] sequence, Func<ReadOnlySequence<byte>, Task> func, int maxLength, CancellationToken cancellationToken)
        {
            if (reader == null)
            {
                throw new ArgumentNullException(nameof(reader));
            }

            var read = await reader.ReadAsync(cancellationToken);
            var head = read.Buffer.Start;

            while (read.IsCanceled == false && read.IsCompleted == false && read.Buffer.IsEmpty == false)
            {
                if (read.Buffer.TryFind(sequence, ref head, out var tail))
                {
                    var line = read.Buffer.Slice(read.Buffer.Start, head);
                    if (line.Length > maxLength)
                    {
                        throw new SmtpResponseException(new SmtpResponse(SmtpReplyCode.SyntaxError, "command line length exceeds maximum command line length"), true);
                    }

                    try
                    {
                        await func(line);
                    }
                    finally
                    {
                        reader.AdvanceTo(tail);
                    }

                    return;
                }

                if (read.Buffer.Length > maxLength)
                {
                    throw new SmtpResponseException(new SmtpResponse(SmtpReplyCode.SyntaxError, "command line length exceeds maximum command line length"), true);
                }

                reader.AdvanceTo(read.Buffer.Start, read.Buffer.End);

                read = await reader.ReadAsync(cancellationToken);
            }
        }

        /// <summary>
        /// Reads a line from the reader.
        /// </summary>
        /// <param name="reader">The reader to read from.</param>
        /// <param name="func">The action to process the buffer.</param>
        /// <param name="maxMessageSizeOptions">Handling of MaxMessageSize.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task that can be used to wait on the operation on complete.</returns>
        internal static ValueTask ReadLineAsync(this PipeReader reader, Func<ReadOnlySequence<byte>, Task> func, IMaxMessageSizeOptions maxMessageSizeOptions, CancellationToken cancellationToken = default)
        {
            if (reader == null)
            {
                throw new ArgumentNullException(nameof(reader));
            }

            return ReadUntilAsync(reader, CRLF, func, maxMessageSizeOptions, cancellationToken);
        }

        /// <summary>
        /// Reads a command line from the reader.
        /// </summary>
        /// <param name="reader">The reader to read from.</param>
        /// <param name="func">The action to process the buffer.</param>
        /// <param name="maxLineLength">The maximum line length in bytes, excluding the terminating CRLF.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task that can be used to wait on the operation on complete.</returns>
        internal static ValueTask ReadLineAsync(this PipeReader reader, Func<ReadOnlySequence<byte>, Task> func, int maxLineLength, CancellationToken cancellationToken = default)
        {
            if (reader == null)
            {
                throw new ArgumentNullException(nameof(reader));
            }

            return ReadUntilAsync(reader, CRLF, func, maxLineLength, cancellationToken);
        }

        /// <summary>
        /// Reads a line from the reader.
        /// </summary>
        /// <param name="reader">The reader to read from.</param>
        /// <param name="maxMessageSizeOptions">Handling of MaxMessageSize.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task that can be used to wait on the operation on complete.</returns>
        internal static ValueTask<string> ReadLineAsync(this PipeReader reader, IMaxMessageSizeOptions maxMessageSizeOptions, CancellationToken cancellationToken = default)
        {
            if (reader == null)
            {
                throw new ArgumentNullException(nameof(reader));
            }

            return reader.ReadLineAsync(Encoding.ASCII, maxMessageSizeOptions, cancellationToken);
        }

        /// <summary>
        /// Reads a command line from the reader.
        /// </summary>
        /// <param name="reader">The reader to read from.</param>
        /// <param name="maxLineLength">The maximum line length in bytes, excluding the terminating CRLF.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task that can be used to wait on the operation on complete.</returns>
        internal static ValueTask<string> ReadLineAsync(this PipeReader reader, int maxLineLength, CancellationToken cancellationToken = default)
        {
            if (reader == null)
            {
                throw new ArgumentNullException(nameof(reader));
            }

            return reader.ReadLineAsync(Encoding.ASCII, maxLineLength, cancellationToken);
        }

        /// <summary>
        /// Reads a line from the reader.
        /// </summary>
        /// <param name="reader">The reader to read from.</param>
        /// <param name="encoding">The encoding to use when converting the input.</param>
        /// <param name="maxMessageSizeOptions"> Handling of MaxMessageSize</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task that can be used to wait on the operation on complete.</returns>
        internal static async ValueTask<string> ReadLineAsync(this PipeReader reader, Encoding encoding, IMaxMessageSizeOptions maxMessageSizeOptions, CancellationToken cancellationToken = default)
        {
            if (reader == null)
            {
                throw new ArgumentNullException(nameof(reader));
            }

            var text = string.Empty;

            await reader.ReadLineAsync(
                buffer =>
                {
                    text = StringUtil.Create(buffer, encoding);

                    return Task.CompletedTask;
                },
                maxMessageSizeOptions,
                cancellationToken);

            return text;
        }

        /// <summary>
        /// Reads a command line from the reader.
        /// </summary>
        /// <param name="reader">The reader to read from.</param>
        /// <param name="encoding">The encoding to use when converting the input.</param>
        /// <param name="maxLineLength">The maximum line length in bytes, excluding the terminating CRLF.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task that can be used to wait on the operation on complete.</returns>
        internal static async ValueTask<string> ReadLineAsync(this PipeReader reader, Encoding encoding, int maxLineLength, CancellationToken cancellationToken = default)
        {
            if (reader == null)
            {
                throw new ArgumentNullException(nameof(reader));
            }

            var text = string.Empty;

            await reader.ReadLineAsync(
                buffer =>
                {
                    text = StringUtil.Create(buffer, encoding);

                    return Task.CompletedTask;
                },
                maxLineLength,
                cancellationToken);

            return text;
        }

        /// <summary>
        /// Reads a line from the reader.
        /// </summary>
        /// <param name="reader">The reader to read from.</param>
        /// <param name="func">The action to process the buffer.</param>
        /// <param name="maxMessageSizeOptions">Handling of MaxMessageSize.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The value that was read from the buffer.</returns>
        internal static async ValueTask ReadDotBlockAsync(this PipeReader reader, Func<ReadOnlySequence<byte>, Task> func, IMaxMessageSizeOptions maxMessageSizeOptions, CancellationToken cancellationToken = default)
        {
            if (reader == null)
            {
                throw new ArgumentNullException(nameof(reader));
            }

            await ReadUntilAsync(
                reader,
                DotBlock,
                buffer =>
                {
                    buffer = Unstuff(buffer);

                    return func(buffer);
                },
                maxMessageSizeOptions,
                cancellationToken);

            static ReadOnlySequence<byte> Unstuff(ReadOnlySequence<byte> buffer)
            {
                var head = buffer.GetPosition(0);
                var start = head;
                
                var segments = new ByteArraySegmentList();
                
                while (buffer.TryFind(DotBlockStuffing, ref head, out var tail))
                {
                    var slice = buffer.Slice(start, buffer.GetPosition(3, head));

                    segments.Append(ref slice);

                    start = tail;
                    head = tail;
                }

                var remaining = buffer.Slice(start);
                segments.Append(ref remaining);
                
                return segments.Build();
            }
        }

        /// <summary>
        /// Reads a dot block from the reader and writes the unstuffed content to the writer.
        /// </summary>
        /// <param name="reader">The reader to read from.</param>
        /// <param name="writer">The writer to stream the message content to.</param>
        /// <param name="maxMessageSizeOptions">Handling of MaxMessageSize.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The value that was read from the buffer.</returns>
        internal static async ValueTask ReadDotBlockAsync(this PipeReader reader, PipeWriter writer, IMaxMessageSizeOptions maxMessageSizeOptions, CancellationToken cancellationToken = default)
        {
            if (reader == null)
            {
                throw new ArgumentNullException(nameof(reader));
            }

            if (writer == null)
            {
                throw new ArgumentNullException(nameof(writer));
            }

            Exception error = null;

            try
            {
                await ReadDotBlockAsync(reader, writer, maxMessageSizeOptions, cancellationToken, DotBlockTailLength).ConfigureAwait(false);
            }
            catch (Exception exception)
            {
                error = exception;
                throw;
            }
            finally
            {
                writer.Complete(error);
            }
        }

        static async ValueTask ReadDotBlockAsync(PipeReader reader, PipeWriter writer, IMaxMessageSizeOptions maxMessageSizeOptions, CancellationToken cancellationToken, int tailLength)
        {
            long consumedLength = 0;

            while (true)
            {
                var read = await reader.ReadAsync(cancellationToken).ConfigureAwait(false);
                var buffer = read.Buffer;

                if (buffer.IsEmpty && read.IsCompleted)
                {
                    return;
                }

                var head = buffer.GetPosition(0);
                if (buffer.TryFind(DotBlock, ref head, out var tail))
                {
                    var body = buffer.Slice(buffer.Start, head);

                    EnsureMessageSize(maxMessageSizeOptions, consumedLength + body.Length);

                    await WriteUnstuffedAsync(writer, body, cancellationToken).ConfigureAwait(false);

                    reader.AdvanceTo(tail);
                    return;
                }

                if (read.IsCompleted)
                {
                    reader.AdvanceTo(buffer.End);
                    return;
                }

                if (buffer.Length > tailLength)
                {
                    var safeLength = buffer.Length - tailLength;
                    var safeBuffer = buffer.Slice(0, safeLength);

                    EnsureMessageSize(maxMessageSizeOptions, consumedLength + safeBuffer.Length);

                    await WriteUnstuffedAsync(writer, safeBuffer, cancellationToken).ConfigureAwait(false);

                    consumedLength += safeBuffer.Length;

                    reader.AdvanceTo(buffer.GetPosition(safeLength), buffer.End);
                    continue;
                }

                reader.AdvanceTo(buffer.Start, buffer.End);
            }
        }

        static void EnsureMessageSize(IMaxMessageSizeOptions maxMessageSizeOptions, long length)
        {
            if (maxMessageSizeOptions.Handling == MaxMessageSizeHandling.Strict && length > maxMessageSizeOptions.Length)
            {
                throw new SmtpResponseException(SmtpResponse.MaxMessageSizeExceeded, true);
            }
        }

        static async ValueTask WriteUnstuffedAsync(PipeWriter writer, ReadOnlySequence<byte> buffer, CancellationToken cancellationToken)
        {
            var head = buffer.GetPosition(0);
            var start = head;

            while (buffer.TryFind(DotBlockStuffing, ref head, out var tail))
            {
                var slice = buffer.Slice(start, buffer.GetPosition(3, head));

                Write(writer, slice);

                start = tail;
                head = tail;
            }

            Write(writer, buffer.Slice(start));

            await writer.FlushAsync(cancellationToken).ConfigureAwait(false);
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
    }
}

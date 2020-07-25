using System;
using System.Buffers;
using System.IO.Pipelines;
using System.Threading;
using System.Threading.Tasks;

namespace SmtpServer.IO
{
    internal static class PipeReaderExtensions
    {
        // ReSharper disable once InconsistentNaming
        static readonly byte[] CRLF = { 13, 10 };
        static readonly byte[] DotBlock = { 13, 10, 46, 13, 10 };

        ///// <summary>
        ///// Read from the reader until the sequence is found.
        ///// </summary>
        ///// <typeparam name="T">The element type of the return value.</typeparam>
        ///// <param name="reader">The reader to read from.</param>
        ///// <param name="sequence">The sequence to find to terminate the read operation.</param>
        ///// <param name="func">The function to convert the buffer that was read into the output.</param>
        ///// <param name="cancellationToken">The cancellation token.</param>
        ///// <returns>The value that was read from the buffer.</returns>
        //static async ValueTask<T> ReadUntilAsync<T>(PipeReader reader, byte[] sequence, Func<ReadOnlySequence<byte>, T> func, CancellationToken cancellationToken)
        //{
        //    if (reader == null)
        //    {
        //        throw new ArgumentNullException(nameof(reader));
        //    }

        //    var read = await reader.ReadAsync(cancellationToken);

        //    while (read.IsCanceled == false && read.IsCompleted == false && read.Buffer.IsEmpty == false)
        //    {
        //        if (read.Buffer.TryFind(sequence, out var head, out var tail))
        //        {
        //            var result = func(read.Buffer.Slice(read.Buffer.Start, head));

        //            reader.AdvanceTo(tail);

        //            return result;
        //        }

        //        reader.AdvanceTo(read.Buffer.Start, read.Buffer.End);

        //        read = await reader.ReadAsync(cancellationToken);
        //    }

        //    return default;
        //}

        /// <summary>
        /// Read from the reader until the sequence is found.
        /// </summary>
        /// <param name="reader">The reader to read from.</param>
        /// <param name="sequence">The sequence to find to terminate the read operation.</param>
        /// <param name="func">The callback to execute to process the buffer.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The value that was read from the buffer.</returns>
        static async ValueTask ReadUntilAsync(PipeReader reader, byte[] sequence, Func<ReadOnlySequence<byte>, Task> func, CancellationToken cancellationToken)
        {
            if (reader == null)
            {
                throw new ArgumentNullException(nameof(reader));
            }

            var read = await reader.ReadAsync(cancellationToken);

            while (read.IsCanceled == false && read.IsCompleted == false && read.Buffer.IsEmpty == false)
            {
                if (read.Buffer.TryFind(sequence, out var head, out var tail))
                {
                    await func(read.Buffer.Slice(read.Buffer.Start, head));

                    reader.AdvanceTo(tail);

                    return;
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
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task that can be used to wait on the operation on complete.</returns>
        internal static ValueTask ReadLineAsync(this PipeReader reader, Func<ReadOnlySequence<byte>, Task> func, CancellationToken cancellationToken)
        {
            if (reader == null)
            {
                throw new ArgumentNullException(nameof(reader));
            }

            return ReadUntilAsync(reader, CRLF, func, cancellationToken);
        }

        /// <summary>
        /// Reads a line from the reader.
        /// </summary>
        /// <param name="reader">The reader to read from.</param>
        /// <param name="func">The action to process the buffer.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The value that was read from the buffer.</returns>
        internal static ValueTask ReadDotBlockAsync(this PipeReader reader, Func<ReadOnlySequence<byte>, Task> func, CancellationToken cancellationToken)
        {
            if (reader == null)
            {
                throw new ArgumentNullException(nameof(reader));
            }

            // TODO: need to perform the Unstuffing of the Dot block

            return ReadUntilAsync(reader, DotBlock, func, cancellationToken);
        }
    }
}
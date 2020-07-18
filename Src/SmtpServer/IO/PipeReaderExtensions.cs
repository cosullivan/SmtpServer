using System;
using System.Buffers;
using System.IO.Pipelines;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SmtpServer.IO
{
    internal static class PipeReaderExtensions
    {
        // ReSharper disable once InconsistentNaming
        static readonly byte[] CRLF = { 13, 10 };

        //static async ValueTask<T> ReadUntilAsync<T>(PipeReader reader, byte[] sequence, Func<ReadOnlySequence<byte>, T> func, CancellationToken cancellationToken)
        //{
        //    if (reader == null)
        //    {
        //        throw new ArgumentNullException(nameof(reader));
        //    }

        //    var read = await reader.ReadAsync(cancellationToken);

        //    while (read.IsCanceled == false && read.IsCompleted == false && read.Buffer.IsEmpty == false)
        //    {
        //        var position = read.Buffer.Find(sequence);

        //        if (position != null)
        //        {
        //            var result = func(read.Buffer.Slice(read.Buffer.Start, position.Value));

        //            reader.AdvanceTo(position.Value);

        //            return result;
        //        }

        //        reader.AdvanceTo(read.Buffer.Start, read.Buffer.End);

        //        read = await reader.ReadAsync(cancellationToken);
        //    }

        //    return default;
        //}

        static async ValueTask<T> ReadUntilAsync<T>(PipeReader reader, byte[] sequence, Func<ReadOnlySequence<byte>, T> func, CancellationToken cancellationToken)
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
                    var result = func(read.Buffer.Slice(read.Buffer.Start, head));

                    reader.AdvanceTo(tail);

                    return result;
                }

                reader.AdvanceTo(read.Buffer.Start, read.Buffer.End);

                read = await reader.ReadAsync(cancellationToken);
            }

            return default;
        }

        //internal static Task<T> ReadDotBlockAsync<T>(this PipeReader reader, Func<ReadOnlySequence<byte>, T> func, CancellationToken cancellationToken)
        //{
        //    HERE: this probably wont work the same as we want to pass the actual buffer to the message handler? or do we use our own buffer here and copy?

        //    throw new NotImplementedException();
        //}

        internal static ValueTask<T> ReadLineAsync<T>(this PipeReader reader, Func<ReadOnlySequence<byte>, T> func, CancellationToken cancellationToken)
        {
            if (reader == null)
            {
                throw new ArgumentNullException(nameof(reader));
            }

            return ReadUntilAsync(reader, CRLF, func, cancellationToken);
        }



        //public static async Task<object> ReadUntilAsync(this PipeReader reader, byte[] sequence, Action<ReadOnlySequence<char>> action, CancellationToken cancellationToken = default)
        //{
        //    if (reader == null)
        //    {
        //        throw new ArgumentNullException(nameof(reader));
        //    }

        //    // TODO: read until the sequence has been found and then process 

        //    // HERE: https://blog.marcgravell.com/2018/07/pipe-dreams-part-1.html

        //    //var read = await reader.ReadAsync(cancellationToken);
        //    //while (read.IsCompleted == false)
        //    //{
        //    //    read.Buffer.PositionOf()
        //    //    read = await reader.ReadAsync(cancellationToken);
        //    //}

        //    throw new NotImplementedException();
        //}

        ///// <summary>
        ///// Read a line from the pipe
        ///// </summary>
        ///// <param name="pipe">The pipe to read a line from.</param>
        ///// <param name="func">The function to parse the buffer that was read.</param>
        ///// <param name="cancellationToken">The cancellation token.</param>
        ///// <returns>The element that was read from the buffer.</returns>
        //public static Task<T> ReadLineAsync<T>(this INetworkPipe pipe, Func<ReadOnlySequence<char>, T> func, CancellationToken cancellationToken = default)
        //{
        //    if (pipe == null)
        //    {
        //        throw new ArgumentNullException(nameof(pipe));
        //    }

        //    //return Trim(await client.ReadUntilAsync(new byte[] { 13, 10 }, cancellationToken).ConfigureAwait(false), new byte[] { 13, 10 });

        //    throw new NotImplementedException();
        //}

        ///// <summary>
        ///// Read a line from the pipe
        ///// </summary>
        ///// <param name="pipe">The pipe to read a line from.</param>
        ///// <param name="encoding">The encoding to use when converting to a string representation.</param>
        ///// <param name="cancellationToken">The cancellation token.</param>
        ///// <returns>The string that was read from the pipe.</returns>
        //public static async Task<string> ReadLineAsync(this INetworkPipe pipe, Encoding encoding, CancellationToken cancellationToken = default)
        //{
        //    if (pipe == null)
        //    {
        //        throw new ArgumentNullException(nameof(pipe));
        //    }

        //    //var blocks = await client.ReadLineAsync(cancellationToken).ConfigureAwait(false);

        //    //return blocks.Count == 0
        //    //    ? null
        //    //    : encoding.GetString(blocks.SelectMany(block => block).ToArray());

        //    throw new NotImplementedException();
        //}
    }
}
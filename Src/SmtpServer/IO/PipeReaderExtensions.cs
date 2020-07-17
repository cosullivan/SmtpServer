using System;
using System.Buffers;
using System.IO.Pipelines;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SmtpServer.IO
{
    public static class PipeReaderExtensions
    {
        public static async Task<object> ReadUntilAsync(this PipeReader reader, byte[] sequence, Action<ReadOnlySequence<char>> action, CancellationToken cancellationToken = default)
        {
            if (reader == null)
            {
                throw new ArgumentNullException(nameof(reader));
            }

            // TODO: read until the sequence has been found and then process 

            // HERE: https://blog.marcgravell.com/2018/07/pipe-dreams-part-1.html

            //var read = await reader.ReadAsync(cancellationToken);
            //while (read.IsCompleted == false)
            //{
            //    read.Buffer.PositionOf()
            //    read = await reader.ReadAsync(cancellationToken);
            //}

            throw new NotImplementedException();
        }

        /// <summary>
        /// Read a line from the pipe
        /// </summary>
        /// <param name="pipe">The pipe to read a line from.</param>
        /// <param name="func">The function to parse the buffer that was read.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The element that was read from the buffer.</returns>
        public static Task<T> ReadLineAsync<T>(this INetworkPipe pipe, Func<ReadOnlySequence<char>, T> func, CancellationToken cancellationToken = default)
        {
            if (pipe == null)
            {
                throw new ArgumentNullException(nameof(pipe));
            }

            //return Trim(await client.ReadUntilAsync(new byte[] { 13, 10 }, cancellationToken).ConfigureAwait(false), new byte[] { 13, 10 });

            throw new NotImplementedException();
        }

        /// <summary>
        /// Read a line from the pipe
        /// </summary>
        /// <param name="pipe">The pipe to read a line from.</param>
        /// <param name="encoding">The encoding to use when converting to a string representation.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The string that was read from the pipe.</returns>
        public static async Task<string> ReadLineAsync(this INetworkPipe pipe, Encoding encoding, CancellationToken cancellationToken = default)
        {
            if (pipe == null)
            {
                throw new ArgumentNullException(nameof(pipe));
            }

            //var blocks = await client.ReadLineAsync(cancellationToken).ConfigureAwait(false);

            //return blocks.Count == 0
            //    ? null
            //    : encoding.GetString(blocks.SelectMany(block => block).ToArray());

            throw new NotImplementedException();
        }
    }
}
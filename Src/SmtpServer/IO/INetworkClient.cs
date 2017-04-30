using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SmtpServer.IO
{
    public interface INetworkClient : IDisposable
    {
        /// <summary>
        /// Returns a series a buffer segments whilst the predicate is satisfied.
        /// </summary>
        /// <param name="predicate">The predicate to apply to the bytes for the continuous segment.</param>
        /// <param name="count">The number of bytes to consume.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The list of buffers that contain the bytes matching while the predicate was true.</returns>
        Task<IReadOnlyList<ArraySegment<byte>>> ReadAsync(Func<byte, bool> predicate, long count, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Write a list of byte array segments.
        /// </summary>
        /// <param name="buffers">The list of array segment buffers to write.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task that asynchronously performs the operation.</returns>
        Task WriteAsync(IReadOnlyList<ArraySegment<byte>> buffers, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Flush the write buffers to the stream.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task that represents the asynchronous flush operation.</returns>
        Task FlushAsync(CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Upgrade to a secure stream.
        /// </summary>
        /// <param name="certificate">The X509Certificate used to authenticate the server.</param>
        /// <param name="protocols">The value that represents the protocol used for authentication.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task that asynchronously performs the operation.</returns>
        Task UpgradeAsync(X509Certificate certificate, SslProtocols protocols, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Returns a value indicating whether or not the stream is secure.
        /// </summary>
        bool IsSecure { get; }
    }

    public static class NetworkClientExtensions
    {
        /// <summary>
        /// Returns a continuous segment of bytes until the given sequence is reached.
        /// </summary>
        /// <param name="stream">The byte stream to perform the operation on.</param>
        /// <param name="sequence">The sequence to match to enable the read operation to complete.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The array segment that defines a continuous segment of characters that have matched the predicate.</returns>
        public static async Task<IReadOnlyList<ArraySegment<byte>>> ReadUntilAsync(this INetworkClient stream, byte[] sequence, CancellationToken cancellationToken)
        {
            if (stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            var found = 0;
            return await stream.ReadAsync(current =>
                {
                    if (found >= sequence.Length)
                    {
                        return false;
                    }

                    if (current == sequence[found])
                    {
                        found++;
                    }
                    else
                    {
                        found = 0;
                    }

                    return true;
                },
                Int32.MaxValue,
                cancellationToken);
        }

        /// <summary>
        /// Read a line from the byte stream.
        /// </summary>
        /// <param name="stream">The stream to read a line from.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The string that was read from the stream.</returns>
        public static Task<string> ReadLineAsync(this INetworkClient stream, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            return ReadLineAsync(stream, Encoding.ASCII, cancellationToken);
        }

        /// <summary>
        /// Read a line from the byte stream.
        /// </summary>
        /// <param name="stream">The stream to read a line from.</param>
        /// <param name="encoding">The encoding to use when converting the bytes to a text representation.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The string that was read from the stream.</returns>
        public static async Task<string> ReadLineAsync(this INetworkClient stream, Encoding encoding, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            var buffers = await stream.ReadUntilAsync(new byte[] { 13, 10 }, cancellationToken);
            var count = buffers.Sum(buffer => buffer.Count);

            return buffers.Count == 0
                ? null
                : encoding.GetString(buffers.SelectMany(buffer => buffer).Take(count - 2).ToArray());
        }

        /// <summary>
        /// Read a blank-line delimated block.
        /// </summary>
        /// <param name="stream">The stream to read a line from.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The buffers that were read until the block was terminated.</returns>
        public static Task<IReadOnlyList<ArraySegment<byte>>> ReadBlockAsync(this INetworkClient stream, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            return stream.ReadUntilAsync(new byte[] { 13, 10, 13, 10 }, cancellationToken);
        }
    }
}
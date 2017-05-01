using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SmtpServer.Protocol;

namespace SmtpServer.IO
{
    public interface INetworkClient : IDisposable
    {
        /// <summary>
        /// Returns a series a buffer segments until the continuation predicate indicates that the method should complete.
        /// </summary>
        /// <param name="continue">The predicate to apply to the byte to determine if the function should continue reading.</param>
        /// <param name="count">The number of bytes to consume.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The list of buffers that contain the bytes matching while the predicate was true.</returns>
        Task<IReadOnlyList<ArraySegment<byte>>> ReadAsync(Func<byte, bool> @continue, long count = Int64.MaxValue, CancellationToken cancellationToken = default(CancellationToken));

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
        /// <param name="client">The byte stream to perform the operation on.</param>
        /// <param name="sequence">The sequence to match to enable the read operation to complete.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The array segment that defines a continuous segment of characters that have matched the predicate.</returns>
        public static async Task<IReadOnlyList<ArraySegment<byte>>> ReadUntilAsync(this INetworkClient client, byte[] sequence, CancellationToken cancellationToken)
        {
            if (client == null)
            {
                throw new ArgumentNullException(nameof(client));
            }

            var found = 0;
            return await client.ReadAsync(current =>
            {
                if (current == sequence[found])
                {
                    found++;
                }
                else
                {
                    found = current == sequence[0] ? 1 : 0;
                }

                return found < sequence.Length;
            },
            cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Read a line from the byte stream.
        /// </summary>
        /// <param name="client">The stream to read a line from.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The string that was read from the stream.</returns>
        public static Task<string> ReadLineAsync(this INetworkClient client, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (client == null)
            {
                throw new ArgumentNullException(nameof(client));
            }

            return ReadLineAsync(client, Encoding.ASCII, cancellationToken);
        }

        /// <summary>
        /// Read a line from the byte stream.
        /// </summary>
        /// <param name="client">The stream to read a line from.</param>
        /// <param name="encoding">The encoding to use when converting the bytes to a text representation.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The string that was read from the stream.</returns>
        public static async Task<string> ReadLineAsync(this INetworkClient client, Encoding encoding, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (client == null)
            {
                throw new ArgumentNullException(nameof(client));
            }

            var buffers = Trim(await client.ReadUntilAsync(new byte[] { 13, 10 }, cancellationToken), 2);

            return buffers.Count == 0
                ? null
                : encoding.GetString(buffers.SelectMany(buffer => buffer).ToArray());
        }

        /// <summary>
        /// Writes a byte array to the underlying client stream.
        /// </summary>
        /// <param name="client">The stream to write the line to.</param>
        /// <param name="buffer">The byte array buffer to write to the client stream.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task which asynchronously performs the operation.</returns>
        public static Task WriteAsync(this INetworkClient client, byte[] buffer, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (client == null)
            {
                throw new ArgumentNullException(nameof(client));
            }

            return client.WriteAsync(new [] { new ArraySegment<byte>(buffer) }, cancellationToken);
        }

        /// <summary>
        /// Writes a line to the client stream.
        /// </summary>
        /// <param name="client">The stream to write the line to.</param>
        /// <param name="text">The text to write to the client stream.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task which asynchronously performs the operation.</returns>
        public static Task WriteLineAsync(this INetworkClient client, string text, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (client == null)
            {
                throw new ArgumentNullException(nameof(client));
            }

            return WriteLineAsync(client, text, Encoding.ASCII, cancellationToken);
        }

        /// <summary>
        /// Read a line from the byte stream.
        /// </summary>
        /// <param name="client">The stream to write the line to.</param>
        /// <param name="text">The text to write to the client stream.</param>
        /// <param name="encoding">The encoding to use when converting the bytes to a text representation.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task which asynchronously performs the operation.</returns>
        public static Task WriteLineAsync(this INetworkClient client, string text, Encoding encoding, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (client == null)
            {
                throw new ArgumentNullException(nameof(client));
            }

            return client.WriteAsync(encoding.GetBytes(text + Environment.NewLine), cancellationToken);
        }

        /// <summary>
        /// Read a blank-line delimated block.
        /// </summary>
        /// <param name="client">The stream to read a line from.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The buffers that were read until the block was terminated.</returns>
        public static async Task<IReadOnlyList<ArraySegment<byte>>> ReadBlockAsync(this INetworkClient client, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (client == null)
            {
                throw new ArgumentNullException(nameof(client));
            }

            return Trim(await client.ReadUntilAsync(new byte[] { 13, 10, 46, 13, 10 }, cancellationToken).ReturnOnAnyThread(), 4);
        }

        /// <summary>
        /// Read a dot terminated block.
        /// </summary>
        /// <param name="client">The stream to read a line from.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The buffers that were read until the block was terminated.</returns>
        public static async Task<IReadOnlyList<ArraySegment<byte>>> ReadDotBlockAsync(this INetworkClient client, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (client == null)
            {
                throw new ArgumentNullException(nameof(client));
            }

            return Trim(await client.ReadUntilAsync(new byte[] { 13, 10, 46, 13, 10 }, cancellationToken).ReturnOnAnyThread(), 5);
        }

        /// <summary>
        /// Trim a given number of bytes from the last segment.
        /// </summary>
        /// <param name="segments">The list of segments to truncate.</param>
        /// <param name="count">The number of bytes to trim.</param>
        /// <returns>The list of segments that have been trimmed by the given amount.</returns>
        static IReadOnlyList<ArraySegment<byte>> Trim(IReadOnlyList<ArraySegment<byte>> segments, int count)
        {
            var list = new List<ArraySegment<byte>>(segments);

            var remaining = count;
            for (var i = list.Count - 1; i >= 0; i--)
            {
                count = Math.Min(remaining, list[i].Count);

                list[i] = Trim(list[i], count);

                remaining -= count;
            }

            return list;
        }

        /// <summary>
        /// Trim a given number of bytes from a segment.
        /// </summary>
        /// <param name="segment">The segment to truncate.</param>
        /// <param name="count">The number of bytes to trim.</param>
        /// <returns>The segment that represents the original segment with the given number of bytes trimmed.</returns>
        static ArraySegment<byte> Trim(ArraySegment<byte> segment, int count)
        {
            return new ArraySegment<byte>(segment.Array, segment.Offset, segment.Count - count);
        }

        /// <summary>
        /// Reply to the client.
        /// </summary>
        /// <param name="client">The text stream to perform the operation on.</param>
        /// <param name="response">The response.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task which performs the operation.</returns>
        public static async Task ReplyAsync(this INetworkClient client, SmtpResponse response, CancellationToken cancellationToken)
        {
            if (client == null)
            {
                throw new ArgumentNullException(nameof(client));
            }

            await client.WriteLineAsync($"{(int)response.ReplyCode} {response.Message}", cancellationToken);
            await client.FlushAsync(cancellationToken);
        }
    }
}
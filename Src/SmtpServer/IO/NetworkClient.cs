using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;

namespace SmtpServer.IO
{
    public sealed class NetworkClient : INetworkClient
    {
        readonly Stream _stream;
        readonly int _bufferLength;
        byte[] _buffer;
        int _bytesRead = -1;
        int _index;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="stream">The stream to return the tokens from.</param>
        /// <param name="bufferLength">The buffer length to read.</param>
        internal NetworkClient(Stream stream, int bufferLength = 64)
        {
            _stream = stream;
            _bufferLength = bufferLength;
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            _stream?.Dispose();
        }

        /// <summary>
        /// Returns a series a buffer segments whilst the predicate is satisfied.
        /// </summary>
        /// <param name="predicate">The predicate to apply to the bytes for the continuous segment.</param>
        /// <param name="count">The number of bytes to consume.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The list of buffers that contain the bytes matching while the predicate was true.</returns>
        public async Task<IReadOnlyList<ArraySegment<byte>>> ReadAsync(Func<byte, bool> predicate, long count, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (await ReadBufferAsync(cancellationToken) == false)
            {
                return new List<ArraySegment<byte>>();
            }

            var segments = new List<ArraySegment<byte>> { Consume(predicate, count) };

            while (_index >= _bytesRead)
            {
                if (await ReadBufferAsync(cancellationToken) == false)
                {
                    return segments;
                }

                if (count <= 0 || predicate(_buffer[0]) == false)
                {
                    return segments;
                }

                segments.Add(Consume(predicate, count));
            }

            return segments;
        }

        /// <summary>
        /// Write a list of byte array segments.
        /// </summary>
        /// <param name="buffers">The list of array segment buffers to write.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task that asynchronously performs the operation.</returns>
        public Task WriteAsync(IReadOnlyList<ArraySegment<byte>> buffers, CancellationToken cancellationToken = default(CancellationToken))
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Flush the write buffers to the stream.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task that represents the asynchronous flush operation.</returns>
        public Task FlushAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Upgrade to a secure stream.
        /// </summary>
        /// <param name="certificate">The X509Certificate used to authenticate the server.</param>
        /// <param name="protocols">The value that represents the protocol used for authentication.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task that asynchronously performs the operation.</returns>
        public Task UpgradeAsync(X509Certificate certificate, SslProtocols protocols, CancellationToken cancellationToken = default(CancellationToken))
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Ensure that the buffer is full for a read operation.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Returns a value indicating whether there was no more data to fill the buffer.</returns>
        async Task<bool> ReadBufferAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (_index >= _bytesRead)
            {
                _index = 0;
                _buffer = new byte[_bufferLength];
                _bytesRead = await _stream.ReadAsync(_buffer, 0, _buffer.Length, cancellationToken).ReturnOnAnyThread();
            }

            return _bytesRead > 0;
        }

        /// <summary>
        /// Returns a continuous segment of characters matching the predicate.
        /// </summary>
        /// <param name="predicate">The predicate to apply to the characters for the continuous segment.</param>
        /// <param name="limit">The limit to the number of characters to consume.</param>
        /// <returns>The array segment that defines a continuous segment of characters that have matched the predicate.</returns>
        ArraySegment<byte> Consume(Func<byte, bool> predicate, long limit)
        {
            var start = _index;

            var current = _buffer[_index];
            while (limit-- > 0 && predicate(current) && ++_index < _bytesRead)
            {
                current = _buffer[_index];
            }

            return new ArraySegment<byte>(_buffer, start, _index - start);
        }

        /// <summary>
        /// Returns a value indicating whether or not the stream is secure.
        /// </summary>
        public bool IsSecure { get; private set; }
    }
}
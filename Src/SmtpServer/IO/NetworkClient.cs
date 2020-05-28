using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SmtpServer.IO
{
    public sealed class NetworkClient : INetworkClient
    {
        readonly int _bufferLength;
        readonly INetworkStream _stream;
        byte[] _buffer;
        int _bytesRead = -1;
        int _index;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="stream">The stream to return the tokens from.</param>
        /// <param name="bufferLength">The buffer length to read.</param>
        internal NetworkClient(INetworkStream stream, int bufferLength)
        {
            _stream = stream;
            _bufferLength = bufferLength;
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            _stream.Dispose();
        }
        
        /// <summary>
        /// Returns a series a buffer segments until the continuation predicate indicates that the method should complete.
        /// </summary>
        /// <param name="continue">The predicate to apply to the byte to determine if the function should continue reading.</param>
        /// <param name="count">The number of bytes to consume.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The list of buffers that contain the bytes matching while the predicate was true.</returns>
        public async Task<IReadOnlyList<ArraySegment<byte>>> ReadAsync(Func<byte, bool> @continue, long count, CancellationToken cancellationToken = default)
        {
            if (await ReadBufferAsync(cancellationToken).ConfigureAwait(false) == false)
            {
                return new List<ArraySegment<byte>>();
            }

            if (TryConsume(@continue, ref count, out var segment) == false)
            {
                return new List<ArraySegment<byte>> { segment };
            }

            var segments = new List<ArraySegment<byte>> { segment };

            while (_index >= _bytesRead && count > 0)
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (await ReadBufferAsync(cancellationToken).ConfigureAwait(false) == false)
                {
                    return segments;
                }

                if (TryConsume(@continue, ref count, out segment) == false)
                {
                    segments.Add(segment);
                    return segments;
                }

                segments.Add(segment);
            }

            return segments;
        }

        /// <summary>
        /// Write a list of byte array segments.
        /// </summary>
        /// <param name="buffers">The list of array segment buffers to write.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task that asynchronously performs the operation.</returns>
        public async Task WriteAsync(IReadOnlyList<ArraySegment<byte>> buffers, CancellationToken cancellationToken = default)
        {
            foreach (var buffer in buffers)
            {
                await _stream.WriteAsync(buffer.Array, buffer.Offset, buffer.Count, cancellationToken).ConfigureAwait(false);

                cancellationToken.ThrowIfCancellationRequested();
            }
        }

        /// <summary>
        /// Flush the write buffers to the stream.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task that represents the asynchronous flush operation.</returns>
        public Task FlushAsync(CancellationToken cancellationToken = default)
        {
            return _stream.FlushAsync(cancellationToken);
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
                _bytesRead = await _stream.ReadAsync(_buffer, 0, _buffer.Length, cancellationToken).ConfigureAwait(false);
            }

            return _bytesRead > 0;
        }

        /// <summary>
        /// Consumes the bytes from the buffer until the continuation function indicates that it should complete.
        /// </summary>
        /// <param name="continue">The continuation function to determine whether the consume operation should stop.</param>
        /// <param name="limit">The limit to the number of bytes to read.</param>
        /// <param name="buffer">The buffer that contains the data that was consumed.</param>
        /// <returns>true if the operation should continue reading, false if not.</returns>
        bool TryConsume(Func<byte, bool> @continue, ref long limit, out ArraySegment<byte> buffer)
        {
            var start = _index;

            var current = _buffer[_index];
            while (limit-- > 0 && ++_index < _bytesRead)
            {
                if (@continue(current) == false)
                {
                    buffer = new ArraySegment<byte>(_buffer, start, _index - start);
                    return false;
                }

                current = _buffer[_index];
            }

            buffer = new ArraySegment<byte>(_buffer, start, _index - start);
            return @continue(current);
        }

        /// <summary>
        /// Returns the underlying Network stream instance.
        /// </summary>
        public INetworkStream Stream => _stream;
    }
}
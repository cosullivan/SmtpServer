using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace SmtpServer.Text
{
    internal sealed class ByteArrayStreamReader
    {
        readonly Stream _stream;
        byte[] _buffer;
        int _bytesRead = -1;
        int _index;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="stream">The stream to return the tokens from.</param>
        /// <param name="bufferLength">The buffer length to read.</param>
        internal ByteArrayStreamReader(Stream stream, int bufferLength = 64)
        {
            _stream = stream;
            _buffer = new byte[bufferLength];
        }

        /// <summary>
        /// Ensure that the buffer is full for a read operation.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Returns a value indicating whether there was no more data to fill the buffer.</returns>
        public Task<bool> ReadBufferAsync(CancellationToken cancellationToken)
        {
            if (_index >= _bytesRead)
            {
                return ReadBufferAsync(new byte[_buffer.Length], cancellationToken);
            }

            return Task.FromResult(true);
        }

        /// <summary>
        /// Ensure that the buffer is full for a read operation.
        /// </summary>
        /// <param name="buffer">The new buffer to use.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Returns a value indicating whether there was no more data to fill the buffer.</returns>
        async Task<bool> ReadBufferAsync(byte[] buffer, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (_index >= _bytesRead)
            {
                _index = 0;
                _buffer = buffer;
                _bytesRead = await _stream.ReadAsync(_buffer, 0, _buffer.Length, cancellationToken).ReturnOnAnyThread();
            }

            return _bytesRead > 0;
        }

        /// <summary>
        /// Returns a continuous segment of characters matching the predicate.
        /// </summary>
        /// <param name="predicate">The predicate to apply to the characters for the continuous segment.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The array segment that defines a continuous segment of characters that have matched the predicate.</returns>
        public Task<IReadOnlyList<ArraySegment<byte>>> ReadWhileAsync(Func<byte, bool> predicate, CancellationToken cancellationToken)
        {
            return ReadWhileAsync(predicate, Int64.MaxValue, cancellationToken);
        }

        /// <summary>
        /// Returns a continuous segment of characters matching the predicate.
        /// </summary>
        /// <param name="predicate">The predicate to apply to the characters for the continuous segment.</param>
        /// <param name="limit">The limit to the number of characters to consume.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The array segment that defines a continuous segment of characters that have matched the predicate.</returns>
        public async Task<IReadOnlyList<ArraySegment<byte>>> ReadWhileAsync(Func<byte, bool> predicate, long limit, CancellationToken cancellationToken)
        {
            var segments = new List<ArraySegment<byte>>();

            while (_index >= _bytesRead)
            {
                if (await ReadBufferAsync(new byte[_buffer.Length], cancellationToken) == false)
                {
                    return segments;
                }

                if (limit <= 0 || predicate(_buffer[0]) == false)
                {
                    return segments;
                }

                segments.Add(Consume(predicate, limit));
            }

            return segments;
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
    }
}
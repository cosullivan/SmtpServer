using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace SmtpServer.Text
{
    public sealed class StreamReader2
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
        public StreamReader2(Stream stream, int bufferLength = 64)
        {
            _stream = stream;
            _buffer = new byte[bufferLength];
        }

        /// <summary>
        /// Ensure the buffer is available for a read operation.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token,</param>
        /// <returns>Returns a value indicating whether the buffer was available for a read operation.</returns>
        public Task<bool> IsBufferAvailableAsync(CancellationToken cancellationToken)
        {
            return ReadBufferAsync(_buffer, cancellationToken);
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
        /// Peek at the next byte in the stream without consuming it.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The next byte in the stream.</returns>
        public Task<int> PeekAsync(CancellationToken cancellationToken)
        {
            if (_index >= _bytesRead)
            {
                return ReadBufferAsync(_buffer, cancellationToken).ContinueWith(t =>
                {
                    if (t.IsCompleted && t.Result)
                    {
                        return _buffer[_index];
                    }

                    return -1;
                },
                cancellationToken);
            }

            return Task.FromResult((int)_buffer[_index]);
        }

        /// <summary>
        /// Read a single byte from the reader.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The byte that was read from the stream.</returns>
        public Task<int> ReadByteAsync(CancellationToken cancellationToken)
        {
            //if (_index >= _bytesRead)
            //{
            //    return EnsureBufferAsync(_buffer, cancellationToken).ContinueWith(t =>
            //    {
            //        if (t.IsCompleted && t.Result)
            //        {
            //            return _buffer[_index];
            //        }

            //        return -1;
            //    },
            //    cancellationToken);
            //}

            //return Task.FromResult((int)_buffer[_index++]);

            var task = PeekAsync(cancellationToken);

            _

            return task;
        }

        /// <summary>
        /// Returns a continuous segment of characters matching the predicate.
        /// </summary>
        /// <param name="predicate">The predicate to apply to the characters for the continuous segment.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The array segment that defines a continuous segment of characters that have matched the predicate.</returns>
        public Task<IReadOnlyList<ArraySegment<byte>>> ReadWhileAsync(Func<byte, bool> predicate, CancellationToken cancellationToken)
        {
            return ReadWhileAsync(predicate, Int32.MaxValue, cancellationToken);
        }

        /// <summary>
        /// Returns a continuous segment of characters matching the predicate.
        /// </summary>
        /// <param name="predicate">The predicate to apply to the characters for the continuous segment.</param>
        /// <param name="limit">The limit to the number of characters to consume.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The array segment that defines a continuous segment of characters that have matched the predicate.</returns>
        public async Task<IReadOnlyList<ArraySegment<byte>>> ReadWhileAsync(Func<byte, bool> predicate, int limit, CancellationToken cancellationToken)
        {
            var segments = new List<ArraySegment<byte>> { Consume(predicate, limit) };

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
        ArraySegment<byte> Consume(Func<byte, bool> predicate, int limit)
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
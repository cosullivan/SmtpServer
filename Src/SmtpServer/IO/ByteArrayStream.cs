using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SmtpServer.IO
{
    internal sealed class ByteArrayStream : Stream
    {
        readonly IReadOnlyList<ArraySegment<byte>> _segments;
        readonly int _length;
        int _index = 0;
        int _position = 0;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="segments">The list of array segments to read from.</param>
        internal ByteArrayStream(IReadOnlyList<ArraySegment<byte>> segments)
        {
            _segments = segments;
            _length = segments.Sum(segment => segment.Count);
        }

        /// <summary>
        /// When overridden in a derived class, clears all buffers for this stream and causes any buffered data to be written to the underlying device.
        /// </summary>
        /// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
        public override void Flush()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// When overridden in a derived class, sets the position within the current stream.
        /// </summary>
        /// <param name="offset">A byte offset relative to the <paramref name="origin" /> parameter. </param>
        /// <param name="origin">A value of type <see cref="T:System.IO.SeekOrigin" /> indicating the reference point used to obtain the new position. </param>
        /// <returns>The new position within the current stream.</returns>
        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// When overridden in a derived class, sets the length of the current stream.
        /// </summary>
        /// <param name="value">The desired length of the current stream in bytes. </param>
        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// When overridden in a derived class, reads a sequence of bytes from the current stream and advances the position within the stream by the number of bytes read.
        /// </summary>
        /// <param name="buffer">An array of bytes. When this method returns, the buffer contains the specified byte array with the values between <paramref name="offset" /> and (<paramref name="offset" /> + <paramref name="count" /> - 1) replaced by the bytes read from the current source. </param>
        /// <param name="offset">The zero-based byte offset in <paramref name="buffer" /> at which to begin storing the data read from the current stream. </param>
        /// <param name="count">The maximum number of bytes to be read from the current stream. </param>
        /// <returns>The total number of bytes read into the buffer. This can be less than the number of bytes requested if that many bytes are not currently available, or zero (0) if the end of the stream has been reached.</returns>
        public override int Read(byte[] buffer, int offset, int count)
        {
            var remaining = count;

            while (remaining > 0 && EnsureDataIsAvailable())
            {
                var length = Math.Min(_segments[_index].Count - _position, remaining);

                Buffer.BlockCopy(_segments[_index].Array, _segments[_index].Offset + _position, buffer, offset + count - remaining, length);

                _position += length;
                remaining -= length;
            }

            return count - remaining;
        }

        /// <summary>
        /// When overridden in a derived class, writes a sequence of bytes to the current stream and advances the current position within this stream by the number of bytes written.
        /// </summary>
        /// <param name="buffer">An array of bytes. This method copies <paramref name="count" /> bytes from <paramref name="buffer" /> to the current stream. </param>
        /// <param name="offset">The zero-based byte offset in <paramref name="buffer" /> at which to begin copying bytes to the current stream. </param>
        /// <param name="count">The number of bytes to be written to the current stream. </param>
        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Ensure that data is available for the operation.
        /// </summary>
        /// <returns>true if there is data available, false if not.</returns>
        bool EnsureDataIsAvailable()
        {
            if (_index < _segments.Count && _position >= _segments[_index].Count)
            {
                _index++;
                _position = 0;
            }

            return _index < _segments.Count;
        }

        /// <summary>
        /// When overridden in a derived class, gets a value indicating whether the current stream supports reading.
        /// </summary>
        /// <returns>true if the stream supports reading; otherwise, false.</returns>
        public override bool CanRead => true;

        /// <summary>
        /// When overridden in a derived class, gets a value indicating whether the current stream supports seeking.
        /// </summary>
        /// <returns>true if the stream supports seeking; otherwise, false.</returns>
        public override bool CanSeek => true;

        /// <summary>
        /// When overridden in a derived class, gets a value indicating whether the current stream supports writing.
        /// </summary>
        /// <returns>true if the stream supports writing; otherwise, false.</returns>
        public override bool CanWrite => false;

        /// <summary>
        /// When overridden in a derived class, gets the length in bytes of the stream.
        /// </summary>
        /// <returns>A long value representing the length of the stream in bytes.</returns>
        public override long Length => _length;

        /// <summary>
        /// When overridden in a derived class, gets or sets the position within the current stream.
        /// </summary>
        /// <returns>The current position within the stream.</returns>
        public override long Position
        {
            get
            {
                var position = 0;

                for (var i = 0; i < _index; i++)
                {
                    position += _segments[i].Count;
                }

                return position + _position;
            }
            set
            {
                var position = (int)value;

                for (_index = 0; _index < _segments.Count; _index++)
                {
                    if (position < _segments[_index].Count)
                    {
                        break;
                    }

                    position -= _segments[_index].Count;
                }

                _position = position;
            }
        }
    }
}
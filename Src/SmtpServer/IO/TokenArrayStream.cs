using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SmtpServer.Text;

namespace SmtpServer.IO
{
    internal sealed class TokenArrayStream : Stream
    {
        readonly IReadOnlyList<Token> _tokens;
        int _tokenIndex = 0;
        int _index = 0;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="tokens">The list of tokens to use in the underlying stream.</param>
        /// <param name="offset">The offset into the list of tokens to start from.</param>
        /// <param name="count">The number of tokens to use in the underlying stream.</param>
        internal TokenArrayStream(IReadOnlyList<Token> tokens, int offset, int count)
        {
            _tokens = tokens.Skip(offset).Take(count).ToList();

            Length = _tokens.Sum(TokenLength);
        }

        /// <summary>
        /// Returns the length of a token.
        /// </summary>
        /// <param name="token">The token to return the length of.</param>
        /// <returns>The length of the token.</returns>
        static int TokenLength(Token token)
        {
            switch (token.Kind)
            {
                case TokenKind.Text:
                case TokenKind.Number:
                    return token.Text.Length;

                case TokenKind.Other:
                case TokenKind.Space:
                    return 1;
                
                case TokenKind.NewLine:
                    return 2;
            }

            return 0;
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
            //var remaining = count;

            //while (remaining > 0 && _segmentIndex < _segments.Count)
            //{
            //    var segment = _segments[_segmentIndex];

            //    if (_index >= segment.Count)
            //    {
            //        _index = 0;
            //        _segmentIndex++;

            //        continue;
            //    }

            //    var length = Math.Min(segment.Count - _index, remaining);

            //    Buffer.BlockCopy(segment.Array, segment.Offset + _index, buffer, offset + count - remaining, length);

            //    _index += length;
            //    remaining -= length;
            //}

            //return count - remaining;

            var remaining = count;

            while (remaining > 0 && _tokenIndex < _tokens.Count)
            {
                var token = _tokens[_tokenIndex];

                if (_index >= TokenLength(token))
                {
                    _index = 0;
                    _tokenIndex++;

                    continue;
                }

                var length = Math.Min(TokenLength(token) - _index, remaining);

                //    Buffer.BlockCopy(segment.Array, segment.Offset + _index, buffer, offset + count - remaining, length);

                _index += length;
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
        public override long Length { get; }

        /// <summary>
        /// When overridden in a derived class, gets or sets the position within the current stream.
        /// </summary>
        /// <returns>The current position within the stream.</returns>
        public override long Position
        {
            get
            {
                //var position = 0;

                //for (var i = 0; i < _segmentIndex; i++)
                //{
                //    position += _segments[i].Count;
                //}

                //return position + _index;
                return 0;
            }
            set
            {
                //var position = (int)value;

                //for (_segmentIndex = 0; _segmentIndex < _segments.Count; _segmentIndex++)
                //{
                //    if (position < _segments[_segmentIndex].Count)
                //    {
                //        break;
                //    }

                //    position -= _segments[_segmentIndex].Count;
                //}

                //_index = position;
            }
        }
    }
}
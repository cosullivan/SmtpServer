using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SmtpServer.Text
{
    internal sealed class ByteArrayTokenReader : TokenReader
    {
        readonly IReadOnlyList<ArraySegment<byte>> _segments;
        int _index = 0;
        int _position = 0;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="segments">The list of array segments to read from.</param>
        internal ByteArrayTokenReader(IReadOnlyList<ArraySegment<byte>> segments)
        {
            _segments = segments;
        }

        /// <summary>
        /// Reads the next token.
        /// </summary>
        /// <returns>The next token that was read.</returns>
        public override Token NextToken()
        {
            if (EnsureDataIsAvailable() == false)
            {
                return Token.None;
            }

            if (Token.IsText(Current))
            {
                return TextToken();
            }

            if (Token.IsNumber(Current))
            {
                return NumberToken();
            }

            if (Token.IsCR(Current))
            {
                return NewLineToken();
            }

            return OtherToken(Current);
        }

        /// <summary>
        /// Creates a single character token that represents the given character.
        /// </summary>
        /// <param name="value">The character to create the token for.</param>
        /// <returns>The token that represents the given character.</returns>
        Token OtherToken(byte value)
        {
            _position++;

            if (Token.IsWhiteSpace(value))
            {
                return Token.Create(TokenKind.Space, value);
            }

            return Token.Create(TokenKind.Other, value);
        }

        /// <summary>
        /// Returns a TextValue token from the current position.
        /// </summary>
        /// <returns>The text token that was found at the current position.</returns>
        Token TextToken()
        {
            return Token.Create(Consume(Token.IsText));
        }

        /// <summary>
        /// Returns a Number token from the current position.
        /// </summary>
        /// <returns>The number token that was found at the current position.</returns>
        Token NumberToken()
        {
            return Token.Create(TokenKind.Number, Consume(Token.IsNumber));
        }

        /// <summary>
        /// Returns a New Line token from the current position.
        /// </summary>
        /// <returns>The new line token that was found at the current position.</returns>
        Token NewLineToken()
        {
            var state = 0;
            var text = Consume(b =>
            {
                switch (state)
                {
                    case 0:
                        state = b == 13 ? 1 : 0;
                        return state == 1;
                        
                    case 1:
                        state = b == 10 ? 2 : 0;
                        return state == 2;
                }
                return false;
            });

            return Token.Create(state == 2 ? TokenKind.NewLine : TokenKind.Space, text);
        }

        /// <summary>
        /// Returns a continuous segment of characters matching the predicate.
        /// </summary>
        /// <param name="predicate">The predicate to apply to the characters for the continuous segment.</param>
        /// <returns>The text that defines a continuous segment of characters that have matched the predicate.</returns>
        string Consume(Func<byte, bool> predicate)
        {
            return String.Concat(ConsumeIterator(predicate).Select(segment => Encoding.UTF8.GetString(segment.Array, segment.Offset, segment.Count)));
        }

        /// <summary>
        /// Returns a continuous segment of characters matching the predicate.
        /// </summary>
        /// <param name="predicate">The predicate to apply to the characters for the continuous segment.</param>
        /// <returns>The array segment that defines a continuous segment of characters that have matched the predicate.</returns>
        IEnumerable<ArraySegment<byte>> ConsumeIterator(Func<byte, bool> predicate)
        {
            var @continue = true;
            while (EnsureDataIsAvailable() && @continue)
            {
                if (TryConsume(predicate, out ArraySegment<byte> segment) == false)
                {
                    yield break;
                }

                yield return segment;

                @continue = _position >= _segments[_index].Count;
            }
        }

        /// <summary>
        /// Try to consume from the current segment.
        /// </summary>
        /// <param name="predicate">The predicate to apply to the characters in the segment</param>
        /// <param name="segment">The segment that was matched.</param>
        /// <returns>true if a segment was consumed, false if not.</returns>
        bool TryConsume(Func<byte, bool> predicate, out ArraySegment<byte> segment)
        {
            var current = _segments[_index];
            var start = _position;

            while (_index < _segments.Count && _position < current.Count && predicate(Current))
            {
                _position++;
            }

            segment = new ArraySegment<byte>(current.Array, current.Offset + start, _position - start);

            return segment.Count > 0;
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
        /// Returns the current value for the reader.
        /// </summary>
        public byte Current => _segments[_index].Array[_segments[_index].Offset + _position];
    }
}
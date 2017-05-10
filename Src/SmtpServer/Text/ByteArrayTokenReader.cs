using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SmtpServer.Text
{
    public sealed class ByteArrayTokenReader : TokenReader
    {
        readonly IReadOnlyList<ArraySegment<byte>> _segments;
        readonly int _length;
        int _index = 0;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="segments">The list of array segments to read from.</param>
        public ByteArrayTokenReader(IReadOnlyList<ArraySegment<byte>> segments)
        {
            _segments = segments;
            _length = segments.Sum(segment => segment.Count);
        }

        /// <summary>
        /// Returns the byte at the given index.
        /// </summary>
        /// <param name="index">The index to return the byte.</param>
        /// <returns>The byte that exists at the given index.</returns>
        byte ElementAt(int index)
        {
            for (var i = 0; i < _segments.Count; i++)
            {
                if (index < _segments[i].Count)
                {
                    return _segments[i].Array[_segments[i].Offset + index];
                }

                index -= _segments[i].Count;
            }

            throw new InvalidOperationException();
        }

        /// <summary>
        /// Reads the next token.
        /// </summary>
        /// <returns>The next token that was read.</returns>
        public override Token NextToken()
        {
            if (IsEof(_index))
            {
                return Token.None;
            }

            var value = ElementAt(_index);

            if (Token.IsText(value))
            {
                return TextToken();
            }

            if (Token.IsNumber(value))
            {
                return NumberToken();
            }
            
            if (Token.IsCR(value))
            {
                return NewLineToken();
            }

            return SingleCharacterToken(value);
        }

        /// <summary>
        /// Creates a single character token that represents the given character.
        /// </summary>
        /// <param name="value">The character to create the token for.</param>
        /// <returns>The token that represents the given character.</returns>
        Token SingleCharacterToken(byte value)
        {
            _index++;

            if (Token.IsWhiteSpace(value))
            {
                return new Token(TokenKind.Space, (char)value);
            }

            return new Token(TokenKind.Other, (char)value);
        }

        /// <summary>
        /// Returns a Text token from the current position.
        /// </summary>
        /// <returns>The text token that was found at the current position.</returns>
        Token TextToken()
        {
            return CreateToken(TokenKind.Text, Token.IsText);
        }

        /// <summary>
        /// Returns a Number token from the current position.
        /// </summary>
        /// <returns>The number token that was found at the current position.</returns>
        Token NumberToken()
        {
            return CreateToken(TokenKind.Number, Token.IsNumber);
        }

        /// <summary>
        /// Returns a New Line token from the current position.
        /// </summary>
        /// <returns>The new line token that was found at the current position.</returns>
        Token NewLineToken()
        {
            if (IsEof(++_index) && Token.IsLF(ElementAt(_index)) == false)
            {
                return new Token(TokenKind.Space, (char)13);
            }

            _index++;

            return Token.NewLine;
        }

        /// <summary>
        /// Create a token from the given array segments.
        /// </summary>
        /// <param name="kind">The token kind.</param>
        /// <param name="predicate">The predicate to apply to the characters for the continuous segment.</param>
        /// <param name="limit">The limit to the number of characters to consume.</param>
        /// <returns>The token that was created from the given list of array segments.</returns>
        Token CreateToken(TokenKind kind, Func<byte, bool> predicate, int limit = Int32.MaxValue)
        {
            var bytes = ConsumeWhile(predicate, limit).ToArray();

            return new Token(kind, Encoding.ASCII.GetString(bytes));
        }

        /// <summary>
        /// Returns a continuous segment of characters matching the predicate.
        /// </summary>
        /// <param name="predicate">The predicate to apply to the characters for the continuous segment.</param>
        /// <param name="limit">The limit to the number of characters to consume.</param>
        /// <returns>The array segment that defines a continuous segment of characters that have matched the predicate.</returns>
        IEnumerable<byte> ConsumeWhile(Func<byte, bool> predicate, int limit)
        {
            var current = ElementAt(_index);
            while (limit-- > 0 && predicate(current))
            {
                yield return current;

                if (IsEof(++_index))
                {
                    yield break;
                }

                current = ElementAt(_index);
            }
        }

        /// <summary>
        /// Returns a value indicating whether or not the given index is currently at the end of the input stream.
        /// </summary>
        /// <param name="index">The index to test.</param>
        /// <returns>true if the given index is at the end of the input, false if not.</returns>
        bool IsEof(int index)
        {
            return index >= _length;
        }
    }
}
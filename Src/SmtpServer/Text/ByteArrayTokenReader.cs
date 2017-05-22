using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace SmtpServer.Text
{
    internal sealed class ByteArrayTokenReader : TokenReader
    {
        readonly IReadOnlyList<ArraySegment<byte>> _segments;
        readonly int _length;
        int _index = 0;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="segments">The list of array segments to read from.</param>
        internal ByteArrayTokenReader(IReadOnlyList<ArraySegment<byte>> segments)
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
    
    internal sealed class ByteArrayTokenReader2 : TokenReader
    {
        readonly IReadOnlyList<ArraySegment<byte>> _segments;
        int _index = 0;
        int _position = 0;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="segments">The list of array segments to read from.</param>
        internal ByteArrayTokenReader2(IReadOnlyList<ArraySegment<byte>> segments)
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
            //var segment = new ArraySegment<byte>(_segments[_segmentIndex].Array, _segments[_segmentIndex].Offset + _index++, 1);

            _position++;

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
            var state = 0;
            var segments = ConsumeWhile(b =>
            {
                switch (state)
                {
                    case 0:
                        if (b == 13)
                        {
                            state = 1;
                            return true;
                        }
                        break;

                    case 1:
                        if (b == 10)
                        {
                            state = 2;
                            return true;
                        }
                        break;
                }
                return false;
            },
            Int32.MaxValue).ToArray();


            //var sequence = new[] { 13, 10 };
            //var found = 0;

            //var segments = ConsumeWhile(current =>
            //{
            //    if (found < sequence.Length && current == sequence[found])
            //    {
            //        found++;
            //        return true;
            //    }

            //    return false;

            //    //found = current == sequence[found]
            //    //    ? found + 1
            //    //    : current == sequence[0] ? 1 : 0;

            //    //return found == 1;
            //},
            //Int32.MaxValue).ToArray();

            if (state == 1)
            {
                return Token.CR;
            }

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
            var segments = ConsumeWhile(predicate, limit).ToArray();

            //return new Token(kind, Encoding.ASCII.GetString(bytes));

            var bytes = segments.SelectMany(s => s.ToArray()).ToArray();

            return new Token(kind, Encoding.ASCII.GetString(bytes));
        }

        ///// <summary>
        ///// Returns a continuous segment of characters matching the predicate.
        ///// </summary>
        ///// <param name="predicate">The predicate to apply to the characters for the continuous segment.</param>
        ///// <param name="limit">The limit to the number of characters to consume.</param>
        ///// <returns>The array segment that defines a continuous segment of characters that have matched the predicate.</returns>
        //IEnumerable<ArraySegment<byte>> ConsumeWhile(Func<byte, bool> predicate, long limit)
        //{
        //    var @continue = true;
        //    while (EnsureDataIsAvailable() && @continue)
        //    {
        //        if (TryConsume(predicate, limit, out ArraySegment<byte> segment, out @continue))
        //        {
        //            yield return segment;
        //        }
        //    }
        //}

        ///// <summary>
        ///// Try to consume from the current segment.
        ///// </summary>
        ///// <param name="predicate">The predicate to apply to the characters in the segment</param>
        ///// <param name="limit">The limit to the number of bytes to consume.</param>
        ///// <param name="segment">The segment that was matched.</param>
        ///// <param name="continue">A value indicating whether or not the consumption should be continued.</param>
        ///// <returns>true if a segment was consumed, false if not.</returns>
        //bool TryConsume(Func<byte, bool> predicate, long limit, out ArraySegment<byte> segment, out bool @continue)
        //{
        //    var current = _segments[_index];
        //    var start = _position;

        //    while (IsEof == false && _position < current.Count && predicate(Current) && limit-- > 0)
        //    {
        //        _position++;
        //    }

        //    segment = new ArraySegment<byte>(current.Array, current.Offset + start, _position - start);
        //    @continue = _position >= current.Count;

        //    return segment.Count > 0;
        //}

        /// <summary>
        /// Returns a continuous segment of characters matching the predicate.
        /// </summary>
        /// <param name="predicate">The predicate to apply to the characters for the continuous segment.</param>
        /// <param name="limit">The limit to the number of characters to consume.</param>
        /// <returns>The array segment that defines a continuous segment of characters that have matched the predicate.</returns>
        IEnumerable<ArraySegment<byte>> ConsumeWhile(Func<byte, bool> predicate, long limit)
        {
            var @continue = true;
            while (EnsureDataIsAvailable() && @continue)
            {
                if (TryConsume(predicate, limit, out ArraySegment<byte> segment) == false)
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
        /// <param name="limit">The limit to the number of bytes to consume.</param>
        /// <param name="segment">The segment that was matched.</param>
        /// <returns>true if a segment was consumed, false if not.</returns>
        bool TryConsume(Func<byte, bool> predicate, long limit, out ArraySegment<byte> segment)
        {
            var current = _segments[_index];
            var start = _position;

            while (_index < _segments.Count && _position < current.Count && predicate(Current) && limit-- > 0)
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

        ///// <summary>
        ///// Returns a value indicating whether the reader is at the end.
        ///// </summary>
        //public bool IsEof => _index >= _segments.Count;
    }
}
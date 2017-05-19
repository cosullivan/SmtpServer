using System;
using System.Collections.Generic;
using System.Linq;
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
        readonly int _length;
        int _segmentIndex = 0;
        int _index = 0;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="segments">The list of array segments to read from.</param>
        internal ByteArrayTokenReader2(IReadOnlyList<ArraySegment<byte>> segments)
        {
            _segments = segments;
            _length = segments.Sum(segment => segment.Count);
        }

        ///// <summary>
        ///// Returns the byte at the given index.
        ///// </summary>
        ///// <param name="index">The index to return the byte.</param>
        ///// <returns>The byte that exists at the given index.</returns>
        //byte PeekAt(int index)
        //{
        //    for (var i = 0; i < _segments.Count; i++)
        //    {
        //        if (index < _segments[i].Count)
        //        {
        //            return _segments[i].Array[_segments[i].Offset + index];
        //        }

        //        index -= _segments[i].Count;
        //    }

        //    throw new InvalidOperationException();
        //}

        //byte Peek()
        //{
        //    if (_segmentIndex < _segments.Count && _index < _segments[_segmentIndex].Count - 1)
        //    {
        //        return _segments[_segmentIndex].Array[_segments[_segmentIndex].Offset + _index + 1];
        //    }

        //    throw new NotImplementedException();
        //}

        /// <summary>
        /// Returns the value at the current position.
        /// </summary>
        /// <returns>The value at the current position.</returns>
        byte Current()
        {
            return _segments[_segmentIndex].Array[_segments[_segmentIndex].Offset + _index];
        }

        /// <summary>
        /// Reads the next token.
        /// </summary>
        /// <returns>The next token that was read.</returns>
        public override Token NextToken()
        {
            if (EnsureMoreDataIsAvailable() == false)
            {
                return Token.None;
            }

            var value = Current();

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

            return OtherToken(value);
        }

        /// <summary>
        /// Creates a single character token that represents the given character.
        /// </summary>
        /// <param name="value">The character to create the token for.</param>
        /// <returns>The token that represents the given character.</returns>
        Token OtherToken(byte value)
        {
            var segment = new ArraySegment<byte>(_segments[_segmentIndex].Array, _segments[_segmentIndex].Offset + _index++, 1);

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
            //var start = _index;

            //if (_index < _segments[_segmentIndex].Count - 1)
            //{
            //    var segment = new 
            //}

            const int WaitingForCR = 0;
            const int WaitingForLF = 1;
            const int Complete = 2;

            var state = WaitingForCR;

            var segments = ConsumeWhile(b =>
            {
                switch (state)
                {
                    case WaitingForCR:
                        if (b == 13)
                        {
                            state = WaitingForLF;
                            return true;
                        }
                        break;

                    case WaitingForLF:
                        if (b == 10)
                        {
                            state = Complete;
                            return true;
                        }
                        break;
                }
                return false;
            }, 2).ToArray();


            throw new NotImplementedException();
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

        /// <summary>
        /// Read the next segment if it is required.
        /// </summary>
        /// <returns>true if there is more data available, false if not.</returns>
        bool EnsureMoreDataIsAvailable()
        {
            if (_segmentIndex >= _segments.Count)
            {
                return false;
            }

            if (_index >= _segments[_segmentIndex].Count)
            {
                _index = 0;
                _segmentIndex++;
            }

            return _segmentIndex < _segments.Count;
        }
        
        /// <summary>
        /// Returns a continuous segment of characters matching the predicate.
        /// </summary>
        /// <param name="predicate">The predicate to apply to the characters for the continuous segment.</param>
        /// <param name="limit">The limit to the number of characters to consume.</param>
        /// <returns>The array segment that defines a continuous segment of characters that have matched the predicate.</returns>
        IEnumerable<ArraySegment<byte>> ConsumeWhile(Func<byte, bool> predicate, long limit)
        {
            while (EnsureMoreDataIsAvailable())
            {
                var segment = Consume(predicate, ref limit);
                
                if (segment.Count == 0)
                {
                    yield break;
                }

                yield return segment;
            }
        }

        /// <summary>
        /// Consumes the bytes from the buffer until the continuation function indicates that it should complete.
        /// </summary>
        /// <param name="predicate">The continuation function to determine whether the consume operation should stop.</param>
        /// <param name="limit">The limit to the number of bytes to read.</param>
        /// <returns>The array segment that was matched.</returns>
        ArraySegment<byte> Consume(Func<byte, bool> predicate, ref long limit)
        {
            var segment = _segments[_segmentIndex];
            var start = _index;
            
            var b = segment.Array[segment.Offset + _index];
            while (predicate(b) && limit-- > 0 && ++_index < segment.Count)
            {
                b = segment.Array[segment.Offset + _index];
            }

            return new ArraySegment<byte>(segment.Array, segment.Offset + start, _index - start);
        }
    }
}
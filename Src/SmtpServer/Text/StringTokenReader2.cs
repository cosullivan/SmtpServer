using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace SmtpServer.Text
{
    public sealed class StringTokenReader2 : TokenReader
    {
        readonly string _input;
        int _index = 0;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="input">The input string.</param>
        public StringTokenReader2(string input)
        {
            _input = input;
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

            var ch = _input[_index];

            if (Char.IsLetter(ch))
            {
                return TextToken();
            }

            if (Char.IsNumber(ch))
            {
                return NumberToken();
            }

            if (ch == 13)
            {
                return NewLineToken();
            }

            return SingleCharacterToken(ch);
        }

        /// <summary>
        /// Creates a single character token that represents the given character.
        /// </summary>
        /// <param name="ch">The character to create the token for.</param>
        /// <returns>The token that represents the given character.</returns>
        Token SingleCharacterToken(char ch)
        {
            _index++;

            if (Char.IsPunctuation(ch))
            {
                return new Token(TokenKind.Punctuation, ch);
            }

            if (Char.IsSymbol(ch))
            {
                return new Token(TokenKind.Symbol, ch);
            }

            if (Char.IsWhiteSpace(ch))
            {
                return new Token(TokenKind.Space, ch);
            }

            return new Token(TokenKind.Other, ch);
        }

        /// <summary>
        /// Returns a Text token from the current position.
        /// </summary>
        /// <returns>The text token that was found at the current position.</returns>
        Token TextToken()
        {
            return CreateToken(TokenKind.Text, Char.IsLetter);
        }

        /// <summary>
        /// Returns a Number token from the current position.
        /// </summary>
        /// <returns>The number token that was found at the current position.</returns>
        Token NumberToken()
        {
            return CreateToken(TokenKind.Number, Char.IsDigit);
        }

        /// <summary>
        /// Returns a New Line token from the current position.
        /// </summary>
        /// <returns>The new line token that was found at the current position.</returns>
        Token NewLineToken()
        {
            _index++;

            if (IsEof(_index))
            {
                return new Token(TokenKind.Space, (char)13);
            }

            var slice = Consume(ch => ch == 10, 1);
            if (slice.Item2 == 1)
            {
                return Token.NewLine;
            }

            // if we couldnt find an immediate LF then we return the CR by itself.
            return new Token(TokenKind.Space, (char)13);
        }

        /// <summary>
        /// Create a token from the given array segments.
        /// </summary>
        /// <param name="kind">The token kind.</param>
        /// <param name="predicate">The predicate to apply to the characters for the continuous segment.</param>
        /// <param name="limit">The limit to the number of characters to consume.</param>
        /// <returns>The token that was created from the given list of array segments.</returns>
        Token CreateToken(TokenKind kind, Func<char, bool> predicate, int limit = Int32.MaxValue)
        {
            var slice = Consume(predicate, limit);

            return new Token(kind, _input.Substring(slice.Item1, slice.Item2));
        }

        /// <summary>
        /// Returns a continuous segment of characters matching the predicate.
        /// </summary>
        /// <param name="predicate">The predicate to apply to the characters for the continuous segment.</param>
        /// <param name="limit">The limit to the number of characters to consume.</param>
        /// <returns>The array segment that defines a continuous segment of characters that have matched the predicate.</returns>
        Tuple<int, int> Consume(Func<char, bool> predicate, int limit)
        {
            var start = _index;

            var current = (char)_input[_index];
            while (limit-- > 0 && predicate(current) && IsEof(++_index) == false)
            {
                current = _input[_index];
            }

            return Tuple.Create(start, _index - start);
        }

        /// <summary>
        /// Returns a value indicating whether or not the given index is currently at the end of the input string.
        /// </summary>
        /// <param name="index">The index to test.</param>
        /// <returns>true if the given index is at the end of the input, false if not.</returns>
        bool IsEof(int index)
        {
            return index + 1 >= _input.Length;
        }
    }

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

            if (IsText(value))
            {
                return TextToken();
            }

            if (IsNumber(value))
            {
                return NumberToken();
            }
            
            if (value == 13)
            {
                return NewLineToken();
            }

            return SingleCharacterToken(value);
        }

        /// <summary>
        /// Returns a value indicating whether or not the given byte is considered a text character.
        /// </summary>
        /// <param name="value">The value to test.</param>
        /// <returns>true if the value is considered a text character, false if not.</returns>
        static bool IsText(byte value)
        {
            return IsBetween(value, 65, 90) || IsBetween(value, 97, 122);
        }

        /// <summary>
        /// Returns a value indicating whether or not the given byte is considered a digit character.
        /// </summary>
        /// <param name="value">The value to test.</param>
        /// <returns>true if the value is considered a digit character, false if not.</returns>
        static bool IsNumber(byte value)
        {
            return IsBetween(value, 48, 57);
        }

        /// <summary>
        /// Returns a value indicating whether or not the given byte is considered a whitespace.
        /// </summary>
        /// <param name="value">The value to test.</param>
        /// <returns>true if the value is considered a whitespace character, false if not.</returns>
        static bool IsWhiteSpace(byte value)
        {
            return value == 32 || IsBetween(value, 9, 13);
        }

        ///// <summary>
        ///// Returns a value indicating whether or not the given byte is considered punctuation.
        ///// </summary>
        ///// <param name="value">The value to test.</param>
        ///// <returns>true if the value is considered a punctuation character, false if not.</returns>
        //static bool IsPunctuation(byte value)
        //{
        //    return IsBetween(value, 33, 35) 
        //        || IsBetween(value, 37, 42) 
        //        || IsBetween(value, 44, 47) 
        //        || IsBetween(value, 58, 59) 
        //        || IsBetween(value, 63, 64) 
        //        || IsBetween(value, 91, 93)
        //        || value == 95 
        //        || value == 123
        //        || value == 125;
        //}

        /// <summary>
        /// Returns a value indicating whether or not the given value is inclusively between a given range.
        /// </summary>
        /// <param name="value">The value to test.</param>
        /// <param name="low">The lower value of the range.</param>
        /// <param name="high">The higher value of the range.</param>
        /// <returns>true if the value is between the range, false if not.</returns>
        static bool IsBetween(byte value, byte low, byte high)
        {
            return value >= low && value <= high;
        }

        /// <summary>
        /// Creates a single character token that represents the given character.
        /// </summary>
        /// <param name="value">The character to create the token for.</param>
        /// <returns>The token that represents the given character.</returns>
        Token SingleCharacterToken(byte value)
        {
            _index++;

            if (IsWhiteSpace(value))
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
            return CreateToken(TokenKind.Text, IsText);
        }

        /// <summary>
        /// Returns a Number token from the current position.
        /// </summary>
        /// <returns>The number token that was found at the current position.</returns>
        Token NumberToken()
        {
            return CreateToken(TokenKind.Number, IsNumber);
        }

        /// <summary>
        /// Returns a New Line token from the current position.
        /// </summary>
        /// <returns>The new line token that was found at the current position.</returns>
        Token NewLineToken()
        {
            _index++;

            if (IsEof(_index))
            {
                return new Token(TokenKind.Space, (char)13);
            }

            if (ConsumeWhile(ch => ch == 10, 1).Any())
            {
                return Token.NewLine;
            }

            // if we couldnt find an immediate LF then we return the CR by itself.
            return new Token(TokenKind.Space, (char)13);
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
            while (limit-- > 0 && predicate(current) && IsEof(++_index) == false)
            {
                yield return current;

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
            return index + 1 >= _length;
        }
    }
}
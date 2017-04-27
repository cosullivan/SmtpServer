using System;

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
}
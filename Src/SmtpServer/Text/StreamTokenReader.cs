using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SmtpServer.Text
{
    public sealed class StreamTokenReader
    {
        readonly StreamReader2 _reader;
        readonly Encoding _encoding;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="stream">The stream to return the tokens from.</param>
        /// <param name="bufferLength">The buffer length to read.</param>
        public StreamTokenReader(Stream stream, int bufferLength = 64) : this(stream, Encoding.ASCII, bufferLength) { }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="stream">The stream to return the tokens from.</param>
        /// <param name="encoding">The encoding to use for converting the bytes into strings.</param>
        /// <param name="bufferLength">The buffer length to read.</param>
        public StreamTokenReader(Stream stream, Encoding encoding, int bufferLength = 64) : this(new StreamReader2(stream, bufferLength), encoding) { }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="reader">The underlying buffered stream reader.</param>
        /// <param name="encoding">The encoding to use for converting the bytes into strings.</param>
        public StreamTokenReader(StreamReader2 reader, Encoding encoding)
        {
            _reader = reader;
            _encoding = encoding;
        }

        /// <summary>
        /// Reads the next token.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The next token that was read.</returns>
        public async Task<Token> NextTokenAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            if (await _reader.IsBufferAvailableAsync(cancellationToken) == false)
            {
                return Token.None;
            }

            var ch = (char)_reader.Peek();

            if (Char.IsLetter(ch))
            {
                return await TextTokenAsync(cancellationToken).ConfigureAwait(false);
            }

            if (Char.IsNumber(ch))
            {
                return await NumberTokenAsync(cancellationToken).ConfigureAwait(false);
            }

            if (ch == 13)
            {
                return await NewLineTokenAsync(cancellationToken).ConfigureAwait(false);
            }

            return SingleCharacterToken();
        }

        /// <summary>
        /// Creates a single character token that represents the given character.
        /// </summary>
        /// <returns>The token that represents the given character.</returns>
        Token SingleCharacterToken()
        {
            var ch = (char)_reader.Take();

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
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The text token that was found at the current position.</returns>
        async Task<Token> TextTokenAsync(CancellationToken cancellationToken)
        {
            return CreateToken(TokenKind.Text, await _reader.ReadWhileAsync(IsLetter, cancellationToken).ReturnOnAnyThread());
        }

        /// <summary>
        /// Returns a value indicating whether or not the given byte is a letter.
        /// </summary>
        /// <param name="b">The byte to test.</param>
        /// <returns>true if the byte is a letter, false if not.</returns>
        static bool IsLetter(byte b)
        {
            return Char.IsLetter((char) b);
        }

        /// <summary>
        /// Returns a Number token from the current position.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The number token that was found at the current position.</returns>
        async Task<Token> NumberTokenAsync(CancellationToken cancellationToken)
        {
            return CreateToken(TokenKind.Number, await _reader.ReadWhileAsync(IsDigit, cancellationToken).ReturnOnAnyThread());
        }

        /// <summary>
        /// Returns a value indicating whether or not the given byte is a digit.
        /// </summary>
        /// <param name="b">The byte to test.</param>
        /// <returns>true if the byte is a digit, false if not.</returns>
        static bool IsDigit(byte b)
        {
            return Char.IsDigit((char)b);
        }

        /// <summary>
        /// Create a token from the given array segments.
        /// </summary>
        /// <param name="kind">The token kind.</param>
        /// <param name="segments">The list of segments to create the token text from.</param>
        /// <returns>The token that was created from the given list of array segments.</returns>
        Token CreateToken(TokenKind kind, IReadOnlyList<ArraySegment<byte>> segments)
        {
            var text = String.Concat(segments.Select(segment => _encoding.GetString(segment.Array, segment.Offset, segment.Count)));

            return new Token(kind, text);
        }

        /// <summary>
        /// Returns a New Line token from the current position.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The new line token that was found at the current position.</returns>
        async Task<Token> NewLineTokenAsync(CancellationToken cancellationToken)
        {
            _reader.Take();

            if (await _reader.IsBufferAvailableAsync(cancellationToken) == false)
            {
                return new Token(TokenKind.Space, (char)13);
            }

            if (_reader.Peek() == 10)
            {
                _reader.Take();

                return Token.NewLine;
            }

            // if we couldnt fine an immediate LF then we return the CR by itself.
            return new Token(TokenKind.Space, (char)13);
        }
    }
}
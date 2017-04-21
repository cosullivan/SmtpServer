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
        readonly Stream _stream;
        readonly Encoding _encoding;
        byte[] _buffer;
        int _bytesRead = -1;
        int _index;

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
        public StreamTokenReader(Stream stream, Encoding encoding, int bufferLength = 64)
        {
            _stream = stream;
            _encoding = encoding;
            _buffer = new byte[bufferLength];
        }

        /// <summary>
        /// Ensure the buffer is full for a read operation.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token,</param>
        /// <returns>Returns a value indicating whether there was no more data to fill the buffer.</returns>
        Task<bool> EnsureBufferAsync(CancellationToken cancellationToken)
        {
            return EnsureBufferAsync(_buffer, cancellationToken);
        }

        /// <summary>
        /// Ensure that the buffer is full for a read operation.
        /// </summary>
        /// <param name="buffer">The new buffer to use.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Returns a value indicating whether there was no more data to fill the buffer.</returns>
        async Task<bool> EnsureBufferAsync(byte[] buffer, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (_index >= _bytesRead)
            {
                _index = 0;
                _buffer = buffer;
                _bytesRead = await _stream.ReadAsync(_buffer, 0, _buffer.Length, cancellationToken).ConfigureAwait(false);
            }

            return _bytesRead > 0;
        }

        /// <summary>
        /// Reads the next token.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The next token that was read.</returns>
        public async Task<Token> NextTokenAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            if (await EnsureBufferAsync(cancellationToken) == false)
            {
                return Token.None;
            }

            var ch = (char)_buffer[_index];

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
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The text token that was found at the current position.</returns>
        async Task<Token> TextTokenAsync(CancellationToken cancellationToken)
        {
            return CreateToken(TokenKind.Text, await ConsumeAsync(Char.IsLetter, cancellationToken));
        }

        /// <summary>
        /// Returns a Number token from the current position.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The number token that was found at the current position.</returns>
        async Task<Token> NumberTokenAsync(CancellationToken cancellationToken)
        {
            return CreateToken(TokenKind.Number, await ConsumeAsync(Char.IsDigit, cancellationToken));
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
            _index++;

            if (await EnsureBufferAsync(cancellationToken) == false)
            {
                return new Token(TokenKind.Space, (char)13);
            }

            var segments = await ConsumeAsync(ch => ch == 10, 1, cancellationToken);
            if (segments[0].Count == 1)
            {
                return Token.NewLine;
            }

            // if we couldnt fine an immediate LF then we return the CR by itself.
            return new Token(TokenKind.Space, (char)13);
        }

        /// <summary>
        /// Returns a continuous segment of characters matching the predicate.
        /// </summary>
        /// <param name="predicate">The predicate to apply to the characters for the continuous segment.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The array segment that defines a continuous segment of characters that have matched the predicate.</returns>
        Task<IReadOnlyList<ArraySegment<byte>>> ConsumeAsync(Func<char, bool> predicate, CancellationToken cancellationToken)
        {
            return ConsumeAsync(predicate, Int32.MaxValue, cancellationToken);
        }

        /// <summary>
        /// Returns a continuous segment of characters matching the predicate.
        /// </summary>
        /// <param name="predicate">The predicate to apply to the characters for the continuous segment.</param>
        /// <param name="limit">The limit to the number of characters to consume.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The array segment that defines a continuous segment of characters that have matched the predicate.</returns>
        async Task<IReadOnlyList<ArraySegment<byte>>> ConsumeAsync(Func<char, bool> predicate, int limit, CancellationToken cancellationToken)
        {
            var segments = new List<ArraySegment<byte>> { Consume(predicate, limit) };

            while (_index >= _bytesRead)
            {
                if (await EnsureBufferAsync(new byte[_buffer.Length], cancellationToken) == false)
                {
                    return segments;
                }

                if (limit <= 0 || predicate((char)_buffer[0]) == false)
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
        ArraySegment<byte> Consume(Func<char, bool> predicate, int limit)
        {
            var start = _index;

            var current = (char)_buffer[_index];
            while (limit-- > 0 && predicate(current) && ++_index < _bytesRead)
            {
                current = (char)_buffer[_index];
            }

            return new ArraySegment<byte>(_buffer, start, _index - start);
        }
    }
}
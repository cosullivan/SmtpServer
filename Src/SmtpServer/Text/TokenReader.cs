using System;
using System.Buffers;

namespace SmtpServer.Text
{
    /// <summary>
    /// Token Reader
    /// </summary>
    public ref struct TokenReader
    {
        ref struct CheckpointState
        {
            public SequencePosition SpanPosition;
            public ReadOnlySpan<byte> Span;
            public int SpanIndex;
            public int Length;
        }

        /// <summary>
        /// Delegate for the TryMake function.
        /// </summary>
        /// <param name="reader">The token reader to use for the operation.</param>
        /// <returns>true if the make operation was a success, false if not.</returns>
        public delegate bool TryMakeDelegate(ref TokenReader reader);

        /// <summary>
        /// Delegate for the TryMake function to allow for "out" parameters.
        /// </summary>
        /// <typeparam name="TOut">The type of the out parameter.</typeparam>
        /// <param name="reader">The token reader to use for the operation.</param>
        /// <param name="value">The out parameter that was found during the make operation.</param>
        /// <returns>true if the make operation found a parameter, false if not.</returns>
        public delegate bool TryMakeDelegate<TOut>(ref TokenReader reader, out TOut value);

        /// <summary>
        /// Delegate for the TryMake function to allow for "out" parameters.
        /// </summary>
        /// <typeparam name="TOut1">The type of the first out parameter.</typeparam>
        /// <typeparam name="TOut2">The type of the second out parameter.</typeparam>
        /// <param name="reader">The token reader to use for the operation.</param>
        /// <param name="value1">The first out parameter that was found during the make operation.</param>
        /// <param name="value2">The second out parameter that was found during the make operation.</param>
        /// <returns>true if the make operation found a parameter, false if not.</returns>
        public delegate bool TryMakeDelegate<TOut1, TOut2>(ref TokenReader reader, out TOut1 value1, out TOut2 value2);

        readonly ReadOnlySequence<byte> _buffer;
        Token _peek;
        bool _hasPeeked;
        SequencePosition _spanPosition;
        ReadOnlySpan<byte> _span;
        int _spanIndex;
        int _length;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="buffer">The buffer to read from.</param>
        public TokenReader(ReadOnlySequence<byte> buffer)
        {
            _buffer = buffer;
            _spanPosition = _buffer.GetPosition(0);
            _span = default;
            _spanIndex = 0;
            _length = 0;
            _peek = default;
            _hasPeeked = false;
        }
        
        /// <summary>
        /// Create a checkpoint of the current state.
        /// </summary>
        /// <returns>The checkpoint for the current state of the reader.</returns>
        CheckpointState Checkpoint()
        {
            return new CheckpointState
            {
                SpanPosition = _spanPosition,
                Span = _span,
                SpanIndex = _spanIndex,
                Length = _length
            };
        }

        /// <summary>
        /// Rollback the current state to the checkpoint.
        /// </summary>
        /// <param name="checkpoint"></param>
        void Rollback(ref CheckpointState checkpoint)
        {
            _span = checkpoint.Span;
            _spanIndex = checkpoint.SpanIndex;
            _spanPosition = checkpoint.SpanPosition;
            _length = checkpoint.Length;

            _hasPeeked = false;
        }

        /// <summary>
        /// Try to make a callback in a transactional way.
        /// </summary>
        /// <param name="delegate">The callback to perform the match.</param>
        /// <returns>true if the match could be made, false if not.</returns>
        public bool TryMake(TryMakeDelegate @delegate)
        {
            var checkpoint = Checkpoint();

            if (@delegate(ref this) == false)
            {
                Rollback(ref checkpoint);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Try to make a callback in a transactional way.
        /// </summary>
        /// <param name="delegate">The callback to perform the match.</param>
        /// <param name="buffer">The buffer that was made.</param>
        /// <returns>true if the match could be made, false if not.</returns>
        public bool TryMake(TryMakeDelegate @delegate, out ReadOnlySequence<byte> buffer)
        {
            buffer = default;

            var checkpoint = Checkpoint();

            if (@delegate(ref this) == false)
            {
                Rollback(ref checkpoint);
                return false;
            }

            var start = _buffer.GetPosition(checkpoint.Length);
            var end = _buffer.GetPosition(_length);

            buffer = _buffer.Slice(start, end);

            return true;
        }

        /// <summary>
        /// Try to make a callback in a transactional way.
        /// </summary>
        /// <param name="delegate">The callback to perform the match.</param>
        /// <param name="found">The parameter that was returned from the matching function.</param>
        /// <returns>true if the match could be made, false if not.</returns>
        public bool TryMake<TOut>(TryMakeDelegate<TOut> @delegate, out TOut found)
        {
            var checkpoint = Checkpoint();

            if (@delegate(ref this, out found) == false)
            {
                Rollback(ref checkpoint);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Try to make a callback in a transactional way.
        /// </summary>
        /// <param name="delegate">The callback to perform the match.</param>
        /// <param name="value1">The first out parameter that was found during the make operation.</param>
        /// <param name="value2">The second out parameter that was found during the make operation.</param>
        /// <returns>true if the match could be made, false if not.</returns>
        public bool TryMake<TOut1, TOut2>(TryMakeDelegate<TOut1, TOut2> @delegate, out TOut1 value1, out TOut2 value2)
        {
            if (_buffer.IsSingleSegment)
            {
                var checkpoint = Checkpoint();

                if (@delegate(ref this, out value1, out value2) == false)
                {
                    Rollback(ref checkpoint);
                    return false;
                }

                return true;
            }

            throw new NotImplementedException();
        }

        /// <summary>
        /// Peek at the next token in the sequence.
        /// </summary>
        /// <returns>The next token in the sequence.</returns>
        public Token Peek()
        {
            if (_hasPeeked == false)
            {
                _peek = ReadToken();
                _hasPeeked = true;
            }

            return _peek;
        }

        /// <summary>
        /// Take the next token from the sequence.
        /// </summary>
        /// <returns>The next token from the sequence.</returns>
        public Token Take()
        {
            if (_hasPeeked)
            {
                _hasPeeked = false;
                _spanIndex += _peek.Text.Length;
                _length += _peek.Text.Length;

                return _peek;
            }

            var token = ReadToken();

            _spanIndex += token.Text.Length;
            _length += token.Text.Length;

            return token;
        }

        /// <summary>
        /// Skip the tokens.
        /// </summary>
        /// <param name="kind">The token kind to skip.</param>
        public void Skip(TokenKind kind)
        {
            while (Peek().Kind == kind)
            {
                Take();
            }
        }

        /// <summary>
        /// Skip the tokens.
        /// </summary>
        /// <param name="predicate">The predicate to determine whether to skip the tokens.</param>
        public void Skip(Func<TokenKind, bool> predicate)
        {
            while (predicate(Peek().Kind))
            {
                Take();
            }
        }

        /// <summary>
        /// Read a token from the current position in the sequence.
        /// </summary>
        /// <returns>The token that was read from the sequence.</returns>
        Token ReadToken()
        {
            if (_spanIndex >= _span.Length && MoveToNextSpan() == false)
            {
                return default;
            }

            switch (_span[_spanIndex])
            {
                case { } ch when Token.IsText(ch):
                    return new Token(TokenKind.Text, ReadWhile(Token.IsText));

                case { } ch when Token.IsNumber(ch):
                    return new Token(TokenKind.Number, ReadWhile(Token.IsNumber));

                case { } ch when Token.IsWhiteSpace(ch):
                    return new Token(TokenKind.Space, ReadOne());

                case { } ch when ch == '-':
                    return new Token(TokenKind.Hyphen, ReadOne());

                case { } ch when ch == '.':
                    return new Token(TokenKind.Period, ReadOne());

                case { } ch when ch == '[':
                    return new Token(TokenKind.LeftBracket, ReadOne());

                case { } ch when ch == ']':
                    return new Token(TokenKind.RightBracket, ReadOne());

                case { } ch when ch == ':':
                    return new Token(TokenKind.Colon, ReadOne());

                case { } ch when ch == '>':
                    return new Token(TokenKind.GreaterThan, ReadOne());

                case { } ch when ch == '<':
                    return new Token(TokenKind.LessThan, ReadOne());

                case { } ch when ch == ',':
                    return new Token(TokenKind.Comma, ReadOne());

                case { } ch when ch == '@':
                    return new Token(TokenKind.At, ReadOne());

                case { } ch when ch == '"':
                    return new Token(TokenKind.Quote, ReadOne());

                case { } ch when ch == '=':
                    return new Token(TokenKind.Equal, ReadOne());

                case { } ch when ch == '\\':
                    return new Token(TokenKind.Backslash, ReadOne());

                case { } ch when ch == '/':
                    return new Token(TokenKind.Slash, ReadOne());

                case { } ch when ch == '+':
                    return new Token(TokenKind.Plus, ReadOne());
            }

            return new Token(TokenKind.Other, ReadOne());
        }

        /// <summary>
        /// Move to the next span in the sequence.
        /// </summary>
        /// <returns>true if the reader could be moved to the next span, false if not.</returns>
        bool MoveToNextSpan()
        {
            while (_buffer.TryGet(ref _spanPosition, out var memory))
            {
                _span = memory.Span;
                _spanIndex = 0;

                if (_span.Length > 0)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Reads a continual sequence while the predicate is matched.
        /// </summary>
        /// <param name="predicate">The predicate to match against the characters in the buffer.</param>
        /// <returns>The span that was matched.</returns>
        ReadOnlySpan<byte> ReadWhile(Func<byte, bool> predicate)
        {
            var count = 0;

            while (_spanIndex + count < _span.Length && predicate(_span[_spanIndex + count]))
            {
                count++;
            }

            return _span.Slice(_spanIndex, count);
        }

        /// <summary>
        /// Read a single character from the span.
        /// </summary>
        /// <returns>The span that was matched.</returns>
        ReadOnlySpan<byte> ReadOne()
        {
            return _span.Slice(_spanIndex, 1);
        }
    }
}

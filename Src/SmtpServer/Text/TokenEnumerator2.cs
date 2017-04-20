using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SmtpServer.Text
{
    public sealed class TokenEnumerator2 : ITokenEnumerator
    {
        readonly StreamTokenReader _tokenReader;
        readonly Stack<TokenEnumeratorCheckpoint> _checkpoints = new Stack<TokenEnumeratorCheckpoint>();

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="tokenReader">The token reader.</param>
        public TokenEnumerator2(StreamTokenReader tokenReader)
        {
            _tokenReader = tokenReader;
            _checkpoints.Push(new TokenEnumeratorCheckpoint(this));
        }

        /// <summary>
        /// Peek at the next token.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The token at the given number of tokens past the current index, or Token.None if no token exists.</returns>
        public async Task<Token> PeekAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            var checkpoint = _checkpoints.Peek();

            if (checkpoint.Index >= checkpoint.Tokens.Count)
            {
                checkpoint.Tokens.Add(await _tokenReader.NextTokenAsync(cancellationToken));
            }

            return checkpoint.Tokens[checkpoint.Index];
        }

        /// <summary>
        /// Take the given number of tokens.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The last token that was consumed.</returns>
        public async Task<Token> TakeAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            var token = await PeekAsync(cancellationToken);

            var checkpoint = _checkpoints.Peek();
            checkpoint.Index++;

            if (_checkpoints.Count == 1)
            {
                checkpoint.Tokens.RemoveRange(0, checkpoint.Index);
                checkpoint.Index = 0;
            }

            return token;
        }

        /// <summary>
        /// Create a checkpoint that will ensure the tokens are kept in the buffer from this point forward.
        /// </summary>
        /// <returns>A disposable instance that is used to release the checkpoint.</returns>
        public ITokenEnumeratorCheckpoint Checkpoint()
        {
            _checkpoints.Push(new TokenEnumeratorCheckpoint(this));

            return _checkpoints.Peek();
        }

        #region TokenEnumeratorCheckpoint

        class TokenEnumeratorCheckpoint : ITokenEnumeratorCheckpoint
        {
            readonly TokenEnumerator2 _enumerator;

            /// <summary>
            /// Constructor.
            /// </summary>
            /// <param name="enumerator">The enumerator that is being checkpointed.</param>
            public TokenEnumeratorCheckpoint(TokenEnumerator2 enumerator)
            {
                _enumerator = enumerator;

                Tokens = new List<Token>();
            }

            /// <summary>
            /// Rollback to the checkpoint;
            /// </summary>
            public void Rollback()
            {
                var source = _enumerator._checkpoints.Pop();

                var destination = _enumerator._checkpoints.Peek();
                destination.Tokens.AddRange(source.Tokens);
            }

            /// <summary>
            /// Release the checkpoint.
            /// </summary>
            public void Dispose()
            {
                var source = _enumerator._checkpoints.Pop();

                var destination = _enumerator._checkpoints.Peek();
                destination.Tokens.AddRange(source.Tokens);
                destination.Index += source.Index;
            }

            /// <summary>
            /// The tokens for this checkpoint.
            /// </summary>
            internal List<Token> Tokens { get; }

            /// <summary>
            /// The position into the list of tokens.
            /// </summary>
            internal int Index { get; set; }
        }

        #endregion
    }
}
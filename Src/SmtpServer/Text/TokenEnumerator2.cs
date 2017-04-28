using System.Collections.Generic;

namespace SmtpServer.Text
{
    public sealed class TokenEnumerator2 : ITokenEnumerator
    {
        readonly IReadOnlyList<Token> _tokens;
        int _index = -1;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="tokenReader">The token reader to read the tokens from.</param>
        public TokenEnumerator2(TokenReader tokenReader) : this(tokenReader.ToList()) { }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="tokens">The list of tokens that the enumerator is working with.</param>
        public TokenEnumerator2(IReadOnlyList<Token> tokens)
        {
            _tokens = tokens;
        }
        
        /// <summary>
        /// Peek at the next token.
        /// </summary>
        /// <returns>The token at the given number of tokens past the current index, or Token.None if no token exists.</returns>
        public Token Peek()
        {
            return At(_index + 1);
        }

        /// <summary>
        /// Take the given number of tokens.
        /// </summary>
        /// <returns>The last token that was consumed.</returns>
        public Token Take()
        {
            return At(++_index);
        }

        /// <summary>
        /// Take the given number of tokens.
        /// </summary>
        /// <returns>The last token that was consumed.</returns>
        Token At(int index)
        {
            return index < _tokens.Count ? _tokens[index] : Token.None;
        }

        /// <summary>
        /// Create a checkpoint that will ensure the tokens are kept in the buffer from this point forward.
        /// </summary>
        /// <returns>A disposable instance that is used to release the checkpoint.</returns>
        public ITokenEnumeratorCheckpoint Checkpoint()
        {
            return new TokenEnumeratorCheckpoint(this);
        }

        #region TokenEnumeratorCheckpoint

        class TokenEnumeratorCheckpoint : ITokenEnumeratorCheckpoint
        {
            readonly TokenEnumerator2 _enumerator;
            readonly int _index;

            /// <summary>
            /// Constructor.
            /// </summary>
            /// <param name="enumerator">The enumerator that is being checkpointed.</param>
            public TokenEnumeratorCheckpoint(TokenEnumerator2 enumerator)
            {
                _enumerator = enumerator;
                _index = enumerator._index;
            }

            /// <summary>
            /// Rollback to the checkpoint;
            /// </summary>
            public void Rollback()
            {
                _enumerator._index = _index;
            }

            /// <summary>
            /// Release the checkpoint.
            /// </summary>
            public void Dispose() { }
        }

        #endregion
    }
}
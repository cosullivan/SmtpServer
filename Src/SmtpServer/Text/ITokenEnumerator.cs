using System;
using System.Collections.Generic;

namespace SmtpServer.Text
{
    public interface ITokenEnumerator
    {
        /// <summary>
        /// Peek at the next token.
        /// </summary>
        /// <returns>The token at the given number of tokens past the current index, or Token.None if no token exists.</returns>
        Token Peek();

        /// <summary>
        /// Take the given number of tokens.
        /// </summary>
        /// <returns>The last token that was consumed.</returns>
        Token Take();

        /// <summary>
        /// Create a checkpoint that will ensure the tokens are kept in the buffer from this point forward.
        /// </summary>
        /// <returns>A disposable instance that is used to release the checkpoint.</returns>
        ITokenEnumeratorCheckpoint Checkpoint();
        
        /// <summary>
        /// The complete list of tokens.
        /// </summary>
        IReadOnlyList<Token> Tokens { get; }
        
        /// <summary>
        /// Returns the current position of the enumerator.
        /// </summary>
        int Position { get; }
    }

    public static class TokenEnumeratorExtensions
    {
        /// <summary>
        /// Skips tokens in the stream while the given predicate is true.
        /// </summary>
        /// <param name="enumerator">The enumerator to perform the operation on.</param>
        /// <param name="predicate">The predicate to use for evaluating whether or not to consume a token.</param>
        public static void Skip(this ITokenEnumerator enumerator, Func<Token, bool> predicate)
        {
            if (enumerator == null)
            {
                throw new ArgumentNullException(nameof(enumerator));
            }

            while (predicate(enumerator.Peek()))
            {
                enumerator.Take();
            }
        }

        /// <summary>
        /// Skips tokens in the stream while the token kind is the same as the supplied kind.
        /// </summary>
        /// <param name="enumerator">The enumerator to perform the operation on.</param>
        /// <param name="kind">The token kind to test against to determine whether the token should be skipped.</param>
        public static void Skip(this ITokenEnumerator enumerator, TokenKind kind)
        {
            if (enumerator == null)
            {
                throw new ArgumentNullException(nameof(enumerator));
            }

            Skip(enumerator, t => t.Kind == kind);
        }
    }
}
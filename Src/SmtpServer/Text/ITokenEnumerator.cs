using System;

namespace SmtpServer.Text
{
    public interface ITokenEnumerator
    {
        ///// <summary>
        ///// Peek at the next token.
        ///// </summary>
        ///// <param name="cancellationToken">The cancellation token.</param>
        ///// <returns>The token at the given number of tokens past the current index, or Token.None if no token exists.</returns>
        //Task<Token> PeekAsync(CancellationToken cancellationToken = default(CancellationToken));

        ///// <summary>
        ///// Take the given number of tokens.
        ///// </summary>
        ///// <param name="cancellationToken">The cancellation token.</param>
        ///// <returns>The last token that was consumed.</returns>
        //Task<Token> TakeAsync(CancellationToken cancellationToken = default(CancellationToken));

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
    }

    public static class TokenEnumeratorExtensions
    {
        ///// <summary>
        ///// Skips tokens in the stream while the given predicate is true.
        ///// </summary>
        ///// <param name="enumerator">The enumerator to perform the operation on.</param>
        ///// <param name="predicate">The predicate to use for evaluating whether or not to consume a token.</param>
        ///// <param name="cancellationToken">The cancellation token.</param>
        //public static async Task SkipAsync(this ITokenEnumerator enumerator, Func<Token, bool> predicate, CancellationToken cancellationToken = default(CancellationToken))
        //{
        //    if (enumerator == null)
        //    {
        //        throw new ArgumentNullException(nameof(enumerator));
        //    }

        //    while (predicate(await enumerator.PeekAsync(cancellationToken)))
        //    {
        //        cancellationToken.ThrowIfCancellationRequested();

        //        await enumerator.TakeAsync(cancellationToken);
        //    }
        //}

        ///// <summary>
        ///// Skips tokens in the stream while the token kind is the same as the supplied kind.
        ///// </summary>
        ///// <param name="enumerator">The enumerator to perform the operation on.</param>
        ///// <param name="kind">The token kind to test against to determine whether the token should be skipped.</param>
        ///// <param name="cancellationToken">The cancellation token.</param>
        //public static Task SkipAsync(this ITokenEnumerator enumerator, TokenKind kind, CancellationToken cancellationToken = default(CancellationToken))
        //{
        //    if (enumerator == null)
        //    {
        //        throw new ArgumentNullException(nameof(enumerator));
        //    }

        //    return SkipAsync(enumerator, t => t.Kind == kind, cancellationToken);
        //}

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
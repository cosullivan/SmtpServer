using System.Threading;
using System.Threading.Tasks;

namespace SmtpServer.Text
{
    public interface ITokenEnumerator
    {
        /// <summary>
        /// Peek at the next token.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The token at the given number of tokens past the current index, or Token.None if no token exists.</returns>
        Task<Token> PeekAsync(CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Take the given number of tokens.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The last token that was consumed.</returns>
        Task<Token> TakeAsync(CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Create a checkpoint that will ensure the tokens are kept in the buffer from this point forward.
        /// </summary>
        /// <returns>A disposable instance that is used to release the checkpoint.</returns>
        ITokenEnumeratorCheckpoint Checkpoint();
    }
}
using System.Threading;
using System.Threading.Tasks;

namespace SmtpServer.Text
{
    public sealed class TokenEnumerator2 : ITokenEnumerator
    {
        readonly StreamTokenReader _tokenReader;
        Token _peek = default(Token);

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="tokenReader">The token reader.</param>
        public TokenEnumerator2(StreamTokenReader tokenReader)
        {
            _tokenReader = tokenReader;
        }

        /// <summary>
        /// Peek at the next token.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The token at the given number of tokens past the current index, or Token.None if no token exists.</returns>
        public async Task<Token> PeekAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            if (_peek == default(Token))
            {
                _peek = await _tokenReader.NextTokenAsync(cancellationToken);
            }

            return _peek;
        }

        /// <summary>
        /// Take the given number of tokens.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The last token that was consumed.</returns>
        public async Task<Token> TakeAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            var token = _peek;

            if (token == default(Token))
            {
                return await _tokenReader.NextTokenAsync(cancellationToken);
            }

            _peek = default(Token);

            return token;
        }
    }
}
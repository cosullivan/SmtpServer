using System.Threading;
using System.Threading.Tasks;

namespace SmtpServer.Authentication
{
    /// <summary>
    /// Bearer Token Authenticator
    /// </summary>
    public abstract class BearerTokenAuthenticator : IBearerTokenAuthenticator
    {
        /// <summary>
        /// Default Bearer Token Authenticator
        /// </summary>
        public static readonly IBearerTokenAuthenticator Default = new DefaultBearerTokenAuthenticator();

        /// <summary>
        /// Authenticate a user account utilizing a bearer token.
        /// </summary>
        /// <param name="context">The session context.</param>
        /// <param name="user">The user to authenticate.</param>
        /// <param name="bearerToken">The bearer token of the user.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>true if the user is authenticated, false if not.</returns>
        public abstract Task<bool> AuthenticateAsync(
            ISessionContext context,
            string user,
            string bearerToken,
            CancellationToken cancellationToken);

        sealed class DefaultBearerTokenAuthenticator : BearerTokenAuthenticator
        {
            /// <summary>
            /// Authenticate a user account utilizing a bearer token.
            /// </summary>
            /// <param name="context">The session context.</param>
            /// <param name="user">The user to authenticate.</param>
            /// <param name="bearerToken">The bearer token of the user.</param>
            /// <param name="cancellationToken">The cancellation token.</param>
            /// <returns>true if the user is authenticated, false if not.</returns>
            public override Task<bool> AuthenticateAsync(
                ISessionContext context,
                string user,
                string bearerToken,
                CancellationToken cancellationToken)
            {
                return Task.FromResult(true);
            }
        }
    }
}

using System.Threading;
using System.Threading.Tasks;

namespace SmtpServer.Authentication
{
    /// <summary>
    /// User Authenticator
    /// </summary>
    public abstract class UserAuthenticator : IUserAuthenticator
    {
        /// <summary>
        /// Default User Authenticator
        /// </summary>
        public static readonly IUserAuthenticator Default = new DefaultUserAuthenticator();

        /// <summary>
        /// Authenticate a user account.
        /// </summary>
        /// <param name="context">The session context.</param>
        /// <param name="user">The user to authenticate.</param>
        /// <param name="password">The password of the user.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>true if the user is authenticated, false if not.</returns>
        public abstract Task<bool> AuthenticateAsync(
            ISessionContext context, 
            string user, 
            string password, 
            CancellationToken cancellationToken);

        sealed class DefaultUserAuthenticator : UserAuthenticator
        {
            /// <summary>
            /// Authenticate a user account.
            /// </summary>
            /// <param name="context">The session context.</param>
            /// <param name="user">The user to authenticate.</param>
            /// <param name="password">The password of the user.</param>
            /// <param name="cancellationToken">The cancellation token.</param>
            /// <returns>true if the user is authenticated, false if not.</returns>
            public override Task<bool> AuthenticateAsync(
                ISessionContext context,
                string user,
                string password,
                CancellationToken cancellationToken)
            {
                return Task.FromResult(true);
            }
        }
    }
}

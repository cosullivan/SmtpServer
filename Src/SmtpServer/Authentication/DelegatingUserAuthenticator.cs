using System;
using System.Threading;
using System.Threading.Tasks;
using SmtpServer.Authentication;

namespace SmtpServer.Authentication
{
    public sealed class DelegatingUserAuthenticator : UserAuthenticator
    {
        readonly Func<string, string, bool> _delegate;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="delegate">THe delegate to execute for the authentication.</param>
        public DelegatingUserAuthenticator(Func<string, string, bool> @delegate)
        {
            _delegate = @delegate;
        }

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
            return Task.FromResult(_delegate(user, password));
        }
    }
}
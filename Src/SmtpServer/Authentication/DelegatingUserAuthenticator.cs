using System;
using System.Threading;
using System.Threading.Tasks;

namespace SmtpServer.Authentication
{
    /// <summary>
    /// Delegating User Authenticator
    /// </summary>
    public sealed class DelegatingUserAuthenticator : UserAuthenticator
    {
        readonly Func<ISessionContext, string, string, bool> _delegate;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="delegate">THe delegate to execute for the authentication.</param>
        public DelegatingUserAuthenticator(Action<string, string> @delegate) : this(Wrap(@delegate)) { }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="delegate">THe delegate to execute for the authentication.</param>
        public DelegatingUserAuthenticator(Func<string, string, bool> @delegate) : this(Wrap(@delegate)) { }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="delegate">THe delegate to execute for the authentication.</param>
        public DelegatingUserAuthenticator(Func<ISessionContext, string, string, bool> @delegate)
        {
            _delegate = @delegate;
        }

        /// <summary>
        /// Wrap the delegate into a function that is compatible with the signature.
        /// </summary>
        /// <param name="delegate">The delegate to wrap.</param>
        /// <returns>The function that is compatible with the main signature.</returns>
        static Func<ISessionContext, string, string, bool> Wrap(Func<string, string, bool> @delegate)
        {
            return (context, user, password) => @delegate(user, password);
        }

        /// <summary>
        /// Wrap the delegate into a function that is compatible with the signature.
        /// </summary>
        /// <param name="delegate">The delegate to wrap.</param>
        /// <returns>The function that is compatible with the main signature.</returns>
        static Func<ISessionContext, string, string, bool> Wrap(Action<string, string> @delegate)
        {
            return (context, user, password) =>
            {
                @delegate(user, password);

                return true;
            };
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
            return Task.FromResult(_delegate(context, user, password));
        }
    }
}

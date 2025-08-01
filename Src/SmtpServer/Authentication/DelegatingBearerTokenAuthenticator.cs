using System;
using System.Threading;
using System.Threading.Tasks;

namespace SmtpServer.Authentication
{
    /// <summary>
    /// Delegating BearerToken Authenticator
    /// </summary>
    public sealed class DelegatingBearerTokenAuthenticator : BearerTokenAuthenticator
    {
        readonly Func<ISessionContext, string, string, bool> _delegate;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="delegate">THe delegate to execute for the authentication.</param>
        public DelegatingBearerTokenAuthenticator(Action<string, string> @delegate) : this(Wrap(@delegate)) { }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="delegate">THe delegate to execute for the authentication.</param>
        public DelegatingBearerTokenAuthenticator(Func<string, string, bool> @delegate) : this(Wrap(@delegate)) { }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="delegate">THe delegate to execute for the authentication.</param>
        public DelegatingBearerTokenAuthenticator(Func<ISessionContext, string, string, bool> @delegate)
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
            return (context, bearerToken, password) => @delegate(bearerToken, password);
        }

        /// <summary>
        /// Wrap the delegate into a function that is compatible with the signature.
        /// </summary>
        /// <param name="delegate">The delegate to wrap.</param>
        /// <returns>The function that is compatible with the main signature.</returns>
        static Func<ISessionContext, string, string, bool> Wrap(Action<string, string> @delegate)
        {
            return (context, bearerToken, password) =>
            {
                @delegate(bearerToken, password);

                return true;
            };
        }

        /// <summary>
        /// Authenticate a bearerToken account.
        /// </summary>
        /// <param name="context">The session context.</param>
        /// <param name="bearerToken">The bearerToken to authenticate.</param>
        /// <param name="password">The password of the bearerToken.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>true if the bearerToken is authenticated, false if not.</returns>
        public override Task<bool> AuthenticateAsync(
            ISessionContext context,
            string bearerToken,
            string password,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(_delegate(context, bearerToken, password));
        }
    }
}

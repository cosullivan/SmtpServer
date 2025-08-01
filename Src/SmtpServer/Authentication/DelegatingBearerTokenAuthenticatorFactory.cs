using System;

namespace SmtpServer.Authentication
{
    /// <summary>
    /// Delegating Bearer Token Authenticator Factory
    /// </summary>
    public class DelegatingBearerTokenAuthenticatorFactory : IBearerTokenAuthenticatorFactory
    {
        readonly Func<ISessionContext, IBearerTokenAuthenticator> _delegate;

        /// <summary>
        /// Delegating Bearer Authenticator Factory
        /// </summary>
        /// <param name="delegate"></param>
        public DelegatingBearerTokenAuthenticatorFactory(Func<ISessionContext, IBearerTokenAuthenticator> @delegate)
        {
            _delegate = @delegate;
        }

        /// <summary>
        /// Creates an instance of the service for the given session context.
        /// </summary>
        /// <param name="context">The session context.</param>
        /// <returns>The service instance for the session context.</returns>
        public IBearerTokenAuthenticator CreateInstance(ISessionContext context)
        {
            return _delegate(context);
        }

    }
}

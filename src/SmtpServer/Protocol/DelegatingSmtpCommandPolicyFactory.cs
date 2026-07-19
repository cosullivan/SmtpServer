using System;

namespace SmtpServer.Protocol
{
    /// <summary>
    /// Delegating SMTP command policy factory.
    /// </summary>
    public sealed class DelegatingSmtpCommandPolicyFactory : ISmtpCommandPolicyFactory
    {
        readonly Func<ISessionContext, ISmtpCommandPolicy> _delegate;

        /// <summary>
        /// Delegating SMTP command policy factory.
        /// </summary>
        /// <param name="delegate">The factory delegate.</param>
        public DelegatingSmtpCommandPolicyFactory(Func<ISessionContext, ISmtpCommandPolicy> @delegate)
        {
            _delegate = @delegate;
        }

        /// <summary>
        /// Creates an instance of the service for the given session context.
        /// </summary>
        /// <param name="context">The session context.</param>
        /// <returns>The service instance for the session context.</returns>
        public ISmtpCommandPolicy CreateInstance(ISessionContext context)
        {
            return _delegate(context);
        }
    }
}

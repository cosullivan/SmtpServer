using System;

namespace SmtpServer.Storage
{
    public sealed class DelegatingMailboxFilterFactory : IMailboxFilterFactory
    {
        readonly Func<ISessionContext, IMailboxFilter> _delegate;

        public DelegatingMailboxFilterFactory(Func<ISessionContext, IMailboxFilter> @delegate)
        {
            _delegate = @delegate;
        }

        /// <summary>
        /// Creates an instance of the service for the given session context.
        /// </summary>
        /// <param name="context">The session context.</param>
        /// <returns>The service instance for the session context.</returns>
        public IMailboxFilter CreateInstance(ISessionContext context)
        {
            return _delegate(context);
        }
    }
}
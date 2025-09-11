using System;

namespace SmtpServer.Storage
{
    /// <summary>
    /// Delegating MessageStore Factory
    /// </summary>
    public sealed class DelegatingMessageStoreFactory : IMessageStoreFactory
    {
        readonly Func<ISessionContext, IMessageStore> _delegate;

        /// <summary>
        /// Delegating MessageStore Factory
        /// </summary>
        /// <param name="delegate"></param>
        public DelegatingMessageStoreFactory(Func<ISessionContext, IMessageStore> @delegate)
        {
            _delegate = @delegate;
        }

        /// <summary>
        /// Creates an instance of the service for the given session context.
        /// </summary>
        /// <param name="context">The session context.</param>
        /// <returns>The service instance for the session context.</returns>
        public IMessageStore CreateInstance(ISessionContext context)
        {
            return _delegate(context);
        }
    }
}

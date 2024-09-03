using System.Linq;

namespace SmtpServer.Storage
{
    internal sealed class CompositeMailboxFilterFactory : IMailboxFilterFactory
    {
        readonly IMailboxFilterFactory[] _factories;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="factories">The list of factories to run in order.</param>
        public CompositeMailboxFilterFactory(params IMailboxFilterFactory[] factories)
        {
            _factories = factories;
        }

        /// <summary>
        /// Creates an instance of the message box filter.
        /// </summary>
        /// <param name="context">The session context.</param>
        /// <returns>The mailbox filter for the session.</returns>
        public IMailboxFilter CreateInstance(ISessionContext context)
        {
            return new CompositeMailboxFilter(_factories.Select(factory => factory.CreateInstance(context)).ToArray());
        }
    }
}

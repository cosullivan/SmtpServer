using System.Linq;
using System.Threading.Tasks;
using SmtpServer.Mail;

namespace SmtpServer.Storage
{
    internal sealed class CompositeMailboxFilter : IMailboxFilter
    {
        readonly IMailboxFilter[] _filters;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="filters">The list of filters to run in order.</param>
        public CompositeMailboxFilter(params IMailboxFilter[] filters)
        {
            _filters = filters;
        }

        /// <summary>
        /// Creates an instance of the message box filter specifically for this session.
        /// </summary>
        /// <param name="context">The session level context.</param>
        /// <returns>The mailbox filter instance specifically for this session.</returns>
        public IMailboxFilter CreateSessionInstance(ISessionContext context)
        {
            return new CompositeMailboxFilter(_filters.Select(filter => filter.CreateSessionInstance(context)).ToArray());
        }

        /// <summary>
        /// Returns a value indicating whether the given mailbox can be accepted as a sender.
        /// </summary>
        /// <param name="from">The mailbox to test.</param>
        /// <param name="size">The estimated message size to accept.</param>
        /// <returns>The acceptance state of the mailbox.</returns>
        public async Task<MailboxFilterResult> CanAcceptFromAsync(IMailbox @from, int size = 0)
        {
            if (_filters == null || _filters.Any() == false)
            {
                return MailboxFilterResult.Yes;
            }

            var results = await Task.WhenAll(_filters.Select(f => f.CanAcceptFromAsync(@from, size)));

            return results.Max();
        }

        /// <summary>
        /// Returns a value indicating whether the given mailbox can be accepted as a recipient to the given sender.
        /// </summary>
        /// <param name="to">The mailbox to test.</param>
        /// <param name="from">The sender's mailbox.</param>
        /// <returns>The acceptance state of the mailbox.</returns>
        public async Task<MailboxFilterResult> CanDeliverToAsync(IMailbox to, IMailbox @from)
        {
            if (_filters == null || _filters.Any() == false)
            {
                return MailboxFilterResult.Yes;
            }

            var results = await Task.WhenAll(_filters.Select(f => f.CanDeliverToAsync(to, @from)));

            return results.Max();
        }
    }
}
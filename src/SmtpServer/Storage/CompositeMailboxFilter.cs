using System.Linq;
using System.Threading;
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
        /// Returns a value indicating whether the given mailbox can be accepted as a sender.
        /// </summary>
        /// <param name="context">The session context.</param>
        /// <param name="from">The mailbox to test.</param>
        /// <param name="size">The estimated message size to accept.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Returns true if the mailbox is accepted, false if not.</returns>
        public async Task<bool> CanAcceptFromAsync(
            ISessionContext context, 
            IMailbox @from, 
            int size,
            CancellationToken cancellationToken = default)
        {
            if (_filters == null || _filters.Any() == false)
            {
                return true;
            }

            var results = await Task.WhenAll(_filters.Select(f => f.CanAcceptFromAsync(context, @from, size, cancellationToken))).ConfigureAwait(false);

            return results.All(r => r == true);
        }

        /// <summary>
        /// Returns a value indicating whether the given mailbox can be accepted as a recipient to the given sender.
        /// </summary>
        /// <param name="context">The session context.</param>
        /// <param name="to">The mailbox to test.</param>
        /// <param name="from">The sender's mailbox.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Returns true if the mailbox can be delivered to, false if not.</returns>
        public async Task<bool> CanDeliverToAsync(
            ISessionContext context, 
            IMailbox to, 
            IMailbox @from,
            CancellationToken cancellationToken = default)
        {
            if (_filters == null || _filters.Any() == false)
            {
                return true;
            }

            var results = await Task.WhenAll(_filters.Select(f => f.CanDeliverToAsync(context, to, @from, cancellationToken))).ConfigureAwait(false);

            return results.All(r => r == true);
        }
    }
}

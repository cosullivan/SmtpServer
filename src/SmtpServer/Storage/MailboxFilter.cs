using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SmtpServer.Mail;

namespace SmtpServer.Storage
{
    /// <summary>
    /// Mailbox Filter
    /// </summary>
    public abstract class MailboxFilter : IParameterizedMailboxFilter
    {
        /// <summary>
        /// Default Mailbox Filter
        /// </summary>
        public static readonly IMailboxFilter Default = new DefaultMailboxFilter();

        /// <inheritdoc />
        public virtual Task<bool> CanAcceptFromAsync(
            ISessionContext context, 
            IMailbox @from, 
            int size,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(true);
        }

        /// <inheritdoc />
        public virtual Task<bool> CanDeliverToAsync(
            ISessionContext context, 
            IMailbox to, 
            IMailbox @from,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(true);
        }

        /// <inheritdoc />
        public virtual Task<bool> CanDeliverToAsync(
            ISessionContext context,
            IMailbox to,
            IMailbox @from,
            IReadOnlyDictionary<string, string> parameters,
            CancellationToken cancellationToken)
        {
            return CanDeliverToAsync(context, to, @from, cancellationToken);
        }

        sealed class DefaultMailboxFilter : MailboxFilter { }
    }
}

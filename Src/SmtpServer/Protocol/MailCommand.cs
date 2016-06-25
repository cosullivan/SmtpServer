using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SmtpServer.Mail;
using SmtpServer.Storage;

namespace SmtpServer.Protocol
{
    public sealed class MailCommand : SmtpCommand
    {
        readonly IMailbox _address;
        readonly IDictionary<string, string> _parameters;
        readonly IMailboxFilter _filter;
        readonly int _maxMessageSize;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="address">The address.</param>
        /// <param name="parameters">The list of extended (ESMTP) parameters.</param>
        /// <param name="filter">The mailbox filter to test for acceptance of the mailbox.</param>
        /// <param name="maxMessageSize">The maximum message size (0 for no limit).</param>
        public MailCommand(IMailbox address, IDictionary<string, string> parameters, IMailboxFilter filter, int maxMessageSize = 0)
        {
            if (filter == null)
            {
                throw new ArgumentNullException(nameof(filter));
            }

            _address = address;
            _parameters = parameters;
            _filter = filter;
            _maxMessageSize = maxMessageSize;
        }

        /// <summary>
        /// Execute the command.
        /// </summary>
        /// <param name="context">The execution context to operate on.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task which asynchronously performs the execution.</returns>
        public override async Task ExecuteAsync(ISmtpSessionContext context, CancellationToken cancellationToken)
        {
            context.Transaction.Reset();

            // check if a size has been defined
            var size = GetMessageSize();

            // check against the server supplied maximum
            if (_maxMessageSize > 0 && size > _maxMessageSize)
            {
                await context.Text.ReplyAsync(SmtpResponse.SizeLimitExceeded, cancellationToken);
                return;
            }

            switch (await CreateSessionInstance(context).CanAcceptFromAsync(Address, size))
            {
                case MailboxFilterResult.Yes:
                    context.Transaction.From = _address;
                    await context.Text.ReplyAsync(SmtpResponse.Ok, cancellationToken);
                    return;

                case MailboxFilterResult.NoTemporarily:
                    await context.Text.ReplyAsync(SmtpResponse.MailboxUnavailable, cancellationToken);
                    return;

                case MailboxFilterResult.NoPermanently:
                    await context.Text.ReplyAsync(SmtpResponse.MailboxNameNotAllowed, cancellationToken);
                    return;
                
                case MailboxFilterResult.SizeLimitExceeded:
                    await context.Text.ReplyAsync(SmtpResponse.SizeLimitExceeded, cancellationToken);
                    return;
            }

            throw new NotSupportedException("The Acceptance state is not supported.");
        }

        /// <summary>
        /// Creates an instance of the message box filter specifically for this session.
        /// </summary>
        /// <param name="context">The session context information.</param>
        /// <returns>The mailbox filter instance specifically for this session.</returns>
        IMailboxFilter CreateSessionInstance(ISessionContext context)
        {
            return _filter.CreateSessionInstance(context);
        }

        /// <summary>
        /// Gets the estimated message size supplied from the ESMTP command extension.
        /// </summary>
        /// <returns>The estimated message size that was supplied by the client.</returns>
        int GetMessageSize()
        {
            string value;
            if (_parameters.TryGetValue("SIZE", out value) == false)
            {
                return 0;
            }

            int size;
            if (Int32.TryParse(value, out size) == false)
            {
                return 0;
            }

            return size;
        }

        /// <summary>
        /// Gets the address that the mail is from.
        /// </summary>
        public IMailbox Address
        {
            get { return _address; }
        }
    }
}

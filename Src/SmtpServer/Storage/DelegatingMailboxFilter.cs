using System;
using System.Threading;
using System.Threading.Tasks;
using SmtpServer.Mail;

namespace SmtpServer.Storage
{
    /// <summary>
    /// Delegating Mailbox Filter
    /// </summary>
    public sealed class DelegatingMailboxFilter : MailboxFilter
    {
        static readonly Func<ISessionContext, IMailbox, bool> EmptyAcceptDelegate = (context, @from) => true;
        static readonly Func<ISessionContext, IMailbox, IMailbox, bool> EmptyDeliverDelegate = (context, to, @from) => true;

        readonly Func<ISessionContext, IMailbox, bool> _canAcceptDelegate;
        readonly Func<ISessionContext, IMailbox, IMailbox, bool> _canDeliverDelegate;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="canAcceptDelegate">The delegate to accept a mailbox.</param>
        public DelegatingMailboxFilter(Func<IMailbox, bool> canAcceptDelegate)
            : this(Wrap(canAcceptDelegate)) { }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="canAcceptDelegate">The delegate to accept a mailbox.</param>
        public DelegatingMailboxFilter(Func<ISessionContext, IMailbox, bool> canAcceptDelegate) 
            : this(canAcceptDelegate, EmptyDeliverDelegate) { }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="canDeliverDelegate">The delegate to deliver to a mailbox.</param>
        public DelegatingMailboxFilter(Func<IMailbox, IMailbox, bool> canDeliverDelegate)
            : this(EmptyAcceptDelegate, Wrap(canDeliverDelegate)) { }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="canDeliverDelegate">The delegate to deliver to a mailbox.</param>
        public DelegatingMailboxFilter(Func<ISessionContext, IMailbox, IMailbox, bool> canDeliverDelegate)
            : this(EmptyAcceptDelegate, canDeliverDelegate) { }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="canAcceptDelegate">The delegate to accept a mailbox.</param>
        /// <param name="canDeliverDelegate">The delegate to deliver to a mailbox.</param>
        public DelegatingMailboxFilter(
            Func<ISessionContext, IMailbox, bool> canAcceptDelegate,
            Func<ISessionContext, IMailbox, IMailbox, bool> canDeliverDelegate)
        {
            _canAcceptDelegate = canAcceptDelegate;
            _canDeliverDelegate = canDeliverDelegate;
        }

        /// <summary>
        /// Wrap the delegate into a function that is compatible with the signature.
        /// </summary>
        /// <param name="delegate">The delegate to wrap.</param>
        /// <returns>The function that is compatible with the main signature.</returns>
        static Func<ISessionContext, IMailbox, bool> Wrap(Func<IMailbox, bool> @delegate)
        {
            return (context, @from) => @delegate(@from);
        }

        /// <summary>
        /// Wrap the delegate into a function that is compatible with the signature.
        /// </summary>
        /// <param name="delegate">The delegate to wrap.</param>
        /// <returns>The function that is compatible with the main signature.</returns>
        static Func<ISessionContext, IMailbox, IMailbox, bool> Wrap(Func<IMailbox, IMailbox, bool> @delegate)
        {
            return (context, to, @from) => @delegate(to, @from);
        }

        /// <summary>
        /// Returns a value indicating whether the given mailbox can be accepted as a sender.
        /// </summary>
        /// <param name="context">The session context.</param>
        /// <param name="from">The mailbox to test.</param>
        /// <param name="size">The estimated message size to accept.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Returns true if the mailbox is accepted, false if not.</returns>
        public override Task<bool> CanAcceptFromAsync(
            ISessionContext context,
            IMailbox @from,
            int size,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(_canAcceptDelegate(context, @from));
        }

        /// <summary>
        /// Returns a value indicating whether the given mailbox can be accepted as a recipient to the given sender.
        /// </summary>
        /// <param name="context">The session context.</param>
        /// <param name="to">The mailbox to test.</param>
        /// <param name="from">The sender's mailbox.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Returns true if the mailbox can be delivered to, false if not.</returns>
        public override Task<bool> CanDeliverToAsync(
            ISessionContext context,
            IMailbox to,
            IMailbox @from,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(_canDeliverDelegate(context, to, @from));
        }
    }
}

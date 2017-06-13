using System;
using SmtpServer.Protocol;
using System.Threading;
using System.Threading.Tasks;

namespace SmtpServer.Storage
{
    public sealed class DelegatingMessageStore : MessageStore
    {
        readonly Func<ISessionContext, IMessageTransaction, SmtpResponse> _delegate;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="delegate">The delegate to execute.</param>
        public DelegatingMessageStore(Action<IMessageTransaction> @delegate) : this(Wrap(@delegate)) { }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="delegate">The delegate to execute.</param>
        public DelegatingMessageStore(Func<IMessageTransaction, SmtpResponse> @delegate) : this(Wrap(@delegate)) { }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="delegate">The delegate to execute.</param>
        public DelegatingMessageStore(Func<ISessionContext, IMessageTransaction, SmtpResponse> @delegate)
        {
            _delegate = @delegate;
        }

        /// <summary>
        /// Wrap the delegate into a function that is compatible with the signature.
        /// </summary>
        /// <param name="delegate">The delegate to wrap.</param>
        /// <returns>The function that is compatible with the main signature.</returns>
        static Func<ISessionContext, IMessageTransaction, SmtpResponse> Wrap(Action<IMessageTransaction> @delegate)
        {
            return (context, transaction) =>
            {
                @delegate(transaction);

                return SmtpResponse.Ok;
            };
        }

        /// <summary>
        /// Wrap the delegate into a function that is compatible with the signature.
        /// </summary>
        /// <param name="delegate">The delegate to wrap.</param>
        /// <returns>The function that is compatible with the main signature.</returns>
        static Func<ISessionContext, IMessageTransaction, SmtpResponse> Wrap(Func<IMessageTransaction, SmtpResponse> @delegate)
        {
            return (context, transaction) => @delegate(transaction);
        }

        /// <summary>
        /// Save the given message to the underlying storage system.
        /// </summary>
        /// <param name="context">The session context.</param>
        /// <param name="transaction">The SMTP message transaction to store.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The response code to return that indicates the result of the message being saved.</returns>
        public override Task<SmtpResponse> SaveAsync(
            ISessionContext context, 
            IMessageTransaction transaction,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(_delegate(context, transaction));
        }
    }
}
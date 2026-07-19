using System.Collections.Generic;
using System.Collections.ObjectModel;
using SmtpServer.Mail;

namespace SmtpServer
{
    /// <summary>
    /// Smtp Message Transaction
    /// </summary>
    internal sealed class SmtpMessageTransaction : IMessageTransaction, IParameterizedMessageTransaction
    {
        /// <summary>
        /// Reset the current transaction.
        /// </summary>
        public void Reset()
        {
            From = null;
            To = new Collection<IMailbox>();
            Recipients = new Collection<IMessageRecipient>();
            Parameters = new ReadOnlyDictionary<string, string>(new Dictionary<string, string>());
        }

        /// <inheritdoc />
        public IMailbox From { get; set; }

        /// <inheritdoc />
        public IList<IMailbox> To { get; set; } = new Collection<IMailbox>();

        /// <inheritdoc />
        public Collection<IMessageRecipient> Recipients { get; private set; } = new Collection<IMessageRecipient>();

        /// <inheritdoc />
        IReadOnlyList<IMessageRecipient> IParameterizedMessageTransaction.Recipients => Recipients;

        /// <inheritdoc />
        public IReadOnlyDictionary<string, string> Parameters { get; set; } = new ReadOnlyDictionary<string, string>(new Dictionary<string, string>());
    }
}

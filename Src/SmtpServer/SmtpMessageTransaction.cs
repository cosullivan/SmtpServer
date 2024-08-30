using System.Collections.Generic;
using System.Collections.ObjectModel;
using SmtpServer.Mail;

namespace SmtpServer
{
    /// <summary>
    /// Smtp Message Transaction
    /// </summary>
    internal sealed class SmtpMessageTransaction : IMessageTransaction
    {
        /// <summary>
        /// Reset the current transaction.
        /// </summary>
        public void Reset()
        {
            From = null;
            To = new Collection<IMailbox>();
            Parameters = new ReadOnlyDictionary<string, string>(new Dictionary<string, string>());
        }

        /// <inheritdoc />
        public IMailbox From { get; set; }

        /// <inheritdoc />
        public IList<IMailbox> To { get; set; } = new Collection<IMailbox>();

        /// <inheritdoc />
        public IReadOnlyDictionary<string, string> Parameters { get; set; } = new ReadOnlyDictionary<string, string>(new Dictionary<string, string>());
    }
}

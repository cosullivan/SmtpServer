using System.Collections.Generic;
using System.Collections.ObjectModel;
using SmtpServer.Mail;

namespace SmtpServer
{
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

        /// <summary>
        /// Gets or sets the mailbox that is sending the message.
        /// </summary>
        public IMailbox From { get; set; }

        /// <summary>
        /// Gets or sets the collection of mailboxes that the message is to be delivered to.
        /// </summary>
        public IList<IMailbox> To { get; set; } = new Collection<IMailbox>();

        /// <summary>
        /// The list of parameters that were supplied by the client.
        /// </summary>
        public IReadOnlyDictionary<string, string> Parameters { get; set; } = new ReadOnlyDictionary<string, string>(new Dictionary<string, string>());
    }
}
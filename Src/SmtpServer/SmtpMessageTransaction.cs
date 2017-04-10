using System.Collections.Generic;
using System.Collections.ObjectModel;
using SmtpServer.Mail;
using SmtpServer.Content;

namespace SmtpServer
{
    internal sealed class SmtpMessageTransaction : ISmtpMessageTransaction
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        public SmtpMessageTransaction()
        {
            Reset();
        }

        /// <summary>
        /// Reset the current transaction.
        /// </summary>
        public void Reset()
        {
            From = null;
            To = new Collection<IMailbox>();
        }

        /// <summary>
        /// Gets or sets the mailbox that is sending the message.
        /// </summary>
        public IMailbox From { get; set; }

        /// <summary>
        /// Gets or sets the collection of mailboxes that the message is to be delivered to.
        /// </summary>
        public IList<IMailbox> To { get; set; }

        /// <summary>
        /// The message that was read for this transaction.
        /// </summary>
        public IMessage Message { get; set; }
    }
}
using System.Collections.Generic;
using SmtpServer.Mail;

namespace SmtpServer
{
    public interface IMessageTransaction
    {
        /// <summary>
        /// Gets or sets the mailbox that is sending the message.
        /// </summary>
        IMailbox From { get; set; }

        /// <summary>
        /// Gets the collection of mailboxes that the message is to be delivered to.
        /// </summary>
        IList<IMailbox> To { get; }

        /// <summary>
        /// Gets the message that was read.
        /// </summary>
        IMessage Message { get; set; }

        /// <summary>
        /// The list of parameters that were supplied by the client.
        /// </summary>
        IReadOnlyDictionary<string, string> Parameters { get; }
    }
}
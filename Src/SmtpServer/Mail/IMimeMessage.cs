using System.Collections.Generic;
using System.Text;

namespace SmtpServer.Mail
{
    public interface IMimeMessage
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
        /// Gets the MIME content.
        /// </summary>
        StringBuilder Mime { get; }
    }
}

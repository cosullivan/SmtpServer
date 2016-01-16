using System.Collections.Generic;
using System.Text;

namespace SmtpServer.Mail
{
    public class MimeMessage : IMimeMessage
    {
        /// <summary>
        /// Gets or sets the mailbox that is sending the message.
        /// </summary>
        public IMailbox From { get; set; }

        /// <summary>
        /// Gets or sets the collection of mailboxes that the message is to be delivered to.
        /// </summary>
        public IList<IMailbox> To { get; set; }

        /// <summary>
        /// Gets or sets the MIME content.
        /// </summary>
        public StringBuilder Mime { get; set; }
    }
}

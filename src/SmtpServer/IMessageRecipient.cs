using System.Collections.Generic;
using SmtpServer.Mail;

namespace SmtpServer
{
    /// <summary>
    /// Message recipient with the parameters supplied on the RCPT command.
    /// </summary>
    public interface IMessageRecipient
    {
        /// <summary>
        /// Gets the recipient mailbox address.
        /// </summary>
        IMailbox Address { get; }

        /// <summary>
        /// Gets the parameters that were supplied for the recipient.
        /// </summary>
        IReadOnlyDictionary<string, string> Parameters { get; }
    }
}

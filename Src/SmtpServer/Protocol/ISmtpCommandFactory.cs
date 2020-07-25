using SmtpServer.Mail;
using System.Collections.Generic;

namespace SmtpServer.Protocol
{
    public interface ISmtpCommandFactory
    {
        /// <summary>
        /// Create an EHLO command.
        /// </summary>
        /// <param name="domainOrAddress">The domain name or address literal.</param>
        /// <returns>The EHLO command.</returns>
        SmtpCommand CreateEhlo(string domainOrAddress);

        /// <summary>
        /// Create a MAIL command.
        /// </summary>
        /// <param name="address">The Mailbox address that the message is from.</param>
        /// <param name="parameters">The optional parameters for the message.</param>
        /// <returns>The MAIL command.</returns>
        SmtpCommand CreateMail(IMailbox address, IReadOnlyDictionary<string, string> parameters);

        /// <summary>
        /// Create a RCPT command.
        /// </summary>
        /// <param name="address">the address that the mail is to..</param>
        /// <returns>The RCPT command.</returns>
        SmtpCommand CreateRcpt(IMailbox address);

        /// <summary>
        /// Create a DATA command.
        /// </summary>
        /// <returns>The DATA command.</returns>
        SmtpCommand CreateData();
    }
}
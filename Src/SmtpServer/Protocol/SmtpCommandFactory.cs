using System.Collections.Generic;
using SmtpServer.Mail;

namespace SmtpServer.Protocol
{
    public class SmtpCommandFactory : ISmtpCommandFactory
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="options">The options that the server was configured to run with.</param>
        public SmtpCommandFactory(ISmtpServerOptions options)
        {
            Options = options;
        }

        /// <summary>
        /// Create an EHLO command.
        /// </summary>
        /// <param name="domainOrAddress">The domain name or address literal.</param>
        /// <returns>The EHLO command.</returns>
        public virtual SmtpCommand CreateEhlo(string domainOrAddress)
        {
            return new EhloCommand(Options, domainOrAddress);
        }

        /// <summary>
        /// Create a MAIL command.
        /// </summary>
        /// <param name="address">The Mailbox address that the message is from.</param>
        /// <param name="parameters">The optional parameters for the message.</param>
        /// <returns>The MAIL command.</returns>
        public virtual SmtpCommand CreateMail(IMailbox address, IReadOnlyDictionary<string, string> parameters)
        {
            return new MailCommand(Options, address, parameters);
        }

        /// <summary>
        /// Create a RCPT command.
        /// </summary>
        /// <param name="address">the address that the mail is to..</param>
        /// <returns>The RCPT command.</returns>
        public virtual SmtpCommand CreateRcpt(IMailbox address)
        {
            return new RcptCommand(Options, address);
        }

        /// <summary>
        /// Create a DATA command.
        /// </summary>
        /// <returns>The DATA command.</returns>
        public virtual SmtpCommand CreateData()
        {
            return new DataCommand(Options);
        }

        /// <summary>
        /// The options that the server was configured to run with.
        /// </summary>
        protected ISmtpServerOptions Options { get; }
    }
}
using System.Collections.Generic;
using System.Net;
using SmtpServer.Mail;

namespace SmtpServer.Protocol
{
    public class SmtpCommandFactory : ISmtpCommandFactory
    {
        /// <summary>
        /// Create a HELO command.
        /// </summary>
        /// <param name="domainOrAddress">The domain name or address literal.</param>
        /// <returns>The HELO command.</returns>
        public virtual SmtpCommand CreateHelo(string domainOrAddress)
        {
            return new HeloCommand(domainOrAddress);
        }

        /// <summary>
        /// Create a EHLO command.
        /// </summary>
        /// <param name="domainOrAddress">The domain name or address literal.</param>
        /// <returns>The EHLO command.</returns>
        public virtual SmtpCommand CreateEhlo(string domainOrAddress)
        {
            return new EhloCommand(domainOrAddress);
        }

        /// <summary>
        /// Create a MAIL command.
        /// </summary>
        /// <param name="address">The Mailbox address that the message is from.</param>
        /// <param name="parameters">The optional parameters for the message.</param>
        /// <returns>The MAIL command.</returns>
        public virtual SmtpCommand CreateMail(IMailbox address, IReadOnlyDictionary<string, string> parameters)
        {
            return new MailCommand(address, parameters);
        }

        /// <summary>
        /// Create a RCPT command.
        /// </summary>
        /// <param name="address">The address that the mail is to.</param>
        /// <returns>The RCPT command.</returns>
        public virtual SmtpCommand CreateRcpt(IMailbox address)
        {
            return new RcptCommand(address);
        }

        /// <summary>
        /// Create a DATA command.
        /// </summary>
        /// <returns>The DATA command.</returns>
        public virtual SmtpCommand CreateData()
        {
            return new DataCommand();
        }

        /// <summary>
        /// Create a QUIT command.
        /// </summary>
        /// <returns>The QUITcommand.</returns>
        public virtual SmtpCommand CreateQuit()
        {
            return new QuitCommand();
        }

        /// <summary>
        /// Create a NOOP command.
        /// </summary>
        /// <returns>The NOOP command.</returns>
        public virtual SmtpCommand CreateNoop()
        {
            return new NoopCommand();
        }

        /// <summary>
        /// Create a RSET command.
        /// </summary>
        /// <returns>The RSET command.</returns>
        public virtual SmtpCommand CreateRset()
        {
            return new RsetCommand();
        }

        /// <summary>
        /// Create a STARTTLS command.
        /// </summary>
        /// <returns>The STARTTLS command.</returns>
        public virtual SmtpCommand CreateStartTls()
        {
            return new StartTlsCommand();
        }

        /// <summary>
        /// Create a AUTH command.
        /// </summary>
        /// <param name="method">The authentication method.</param>
        /// <param name="parameter">The authentication parameter.</param>
        /// <returns>The AUTH command.</returns>
        public SmtpCommand CreateAuth(AuthenticationMethod method, string parameter)
        {
            return new AuthCommand(method, parameter);
        }

        /// <summary>
        /// Create a PROXY command.
        /// </summary>
        /// <param name="sourceEndpoint">The source endpoint.</param>
        /// <param name="destinationEndpoint">The destination endpoint.</param>
        /// <returns>The PROXY command.</returns>
        public virtual SmtpCommand CreateProxy(IPEndPoint sourceEndpoint, IPEndPoint destinationEndpoint)
        {
            return new ProxyCommand(sourceEndpoint, destinationEndpoint);
        }
    }
}
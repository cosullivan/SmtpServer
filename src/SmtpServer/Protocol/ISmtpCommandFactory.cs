using SmtpServer.Mail;
using System.Collections.Generic;
using System.Net;

namespace SmtpServer.Protocol
{
    /// <summary>
    /// Smtp Command Factory Interface
    /// </summary>
    public interface ISmtpCommandFactory
    {
        /// <summary>
        /// Create a HELO command.
        /// </summary>
        /// <param name="domainOrAddress">The domain name or address literal.</param>
        /// <returns>The HELO command.</returns>
        SmtpCommand CreateHelo(string domainOrAddress);

        /// <summary>
        /// Create a EHLO command.
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
        /// <param name="address">The address that the mail is to.</param>
        /// <returns>The RCPT command.</returns>
        SmtpCommand CreateRcpt(IMailbox address);

        /// <summary>
        /// Create a DATA command.
        /// </summary>
        /// <returns>The DATA command.</returns>
        SmtpCommand CreateData();

        /// <summary>
        /// Create a QUIT command.
        /// </summary>
        /// <returns>The QUIT command.</returns>
        SmtpCommand CreateQuit();

        /// <summary>
        /// Create a NOOP command.
        /// </summary>
        /// <returns>The NOOP command.</returns>
        SmtpCommand CreateNoop();

        /// <summary>
        /// Create a RSET command.
        /// </summary>
        /// <returns>The RSET command.</returns>
        SmtpCommand CreateRset();

        /// <summary>
        /// Create a STARTTLS command.
        /// </summary>
        /// <returns>The STARTTLS command.</returns>
        SmtpCommand CreateStartTls();

        /// <summary>
        /// Create a AUTH command.
        /// </summary>
        /// <param name="method">The authentication method.</param>
        /// <param name="parameter">The authentication parameter.</param>
        /// <returns>The AUTH command.</returns>
        SmtpCommand CreateAuth(AuthenticationMethod method, string parameter);

        /// <summary>
        /// Create a PROXY command.
        /// </summary>
        /// <param name="sourceEndpoint">The source endpoint.</param>
        /// <param name="destinationEndpoint">The destination endpoint.</param>
        /// <returns>The PROXY command.</returns>
        SmtpCommand CreateProxy(IPEndPoint sourceEndpoint = null, IPEndPoint destinationEndpoint = null);
    }
}

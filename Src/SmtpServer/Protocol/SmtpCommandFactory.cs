using System.Collections.Generic;
using System.Net;
using SmtpServer.Authentication;
using SmtpServer.Mail;
using SmtpServer.Storage;

namespace SmtpServer.Protocol
{
    public class SmtpCommandFactory : ISmtpCommandFactory
    {
        readonly ISmtpServerOptions _options;
        readonly IUserAuthenticatorFactory _userAuthenticatorFactory;
        readonly IMailboxFilterFactory _mailboxFilterFactory;
        readonly IMessageStoreFactory _messageStoreFactory;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="options">The options that the server was configured to run with.</param>
        /// <param name="userAuthenticatorFactory">The user authenticator factory.</param>
        /// <param name="mailboxFilterFactory">The mailbox filter factory.</param>
        /// <param name="messageStoreFactory">The message store factory.</param>
        public SmtpCommandFactory(
            ISmtpServerOptions options, 
            IUserAuthenticatorFactory userAuthenticatorFactory,
            IMailboxFilterFactory mailboxFilterFactory,
            IMessageStoreFactory messageStoreFactory)
        {
            _options = options;
            _userAuthenticatorFactory = userAuthenticatorFactory;
            _mailboxFilterFactory = mailboxFilterFactory;
            _messageStoreFactory = messageStoreFactory;
        }

        /// <summary>
        /// Create a HELO command.
        /// </summary>
        /// <param name="domainOrAddress">The domain name or address literal.</param>
        /// <returns>The HELO command.</returns>
        public virtual SmtpCommand CreateHelo(string domainOrAddress)
        {
            var greeting = $"{_options.ServerName} Hello {domainOrAddress}, haven't we met before?";

            return new HeloCommand(domainOrAddress, greeting);
        }

        /// <summary>
        /// Create a EHLO command.
        /// </summary>
        /// <param name="domainOrAddress">The domain name or address literal.</param>
        /// <returns>The EHLO command.</returns>
        public virtual SmtpCommand CreateEhlo(string domainOrAddress)
        {
            var greeting = $"{_options.ServerName} Hello {domainOrAddress}, haven't we met before?";

            return new EhloCommand(domainOrAddress, greeting, GetExtensions);

            IEnumerable<string> GetExtensions(ISessionContext context)
            {
                yield return "PIPELINING";
                yield return "8BITMIME";
                yield return "SMTPUTF8";

                if (context.Pipe.IsSecure == false && _options.ServerCertificate != null)
                {
                    yield return "STARTTLS";
                }

                if (_options.MaxMessageSize > 0)
                {
                    yield return $"SIZE {_options.MaxMessageSize}";
                }

                if (IsPlainLoginAllowed(context))
                {
                    yield return "AUTH PLAIN LOGIN";
                }
            }

            bool IsPlainLoginAllowed(ISessionContext context)
            {
                if (_userAuthenticatorFactory == null)
                {
                    return false;
                }

                return context.Pipe.IsSecure || context.EndpointDefinition.AllowUnsecureAuthentication;
            }
        }

        /// <summary>
        /// Create a MAIL command.
        /// </summary>
        /// <param name="address">The Mailbox address that the message is from.</param>
        /// <param name="parameters">The optional parameters for the message.</param>
        /// <returns>The MAIL command.</returns>
        public virtual SmtpCommand CreateMail(IMailbox address, IReadOnlyDictionary<string, string> parameters)
        {
            return new MailCommand(address, parameters, _mailboxFilterFactory, _options.MaxMessageSize);
        }

        /// <summary>
        /// Create a RCPT command.
        /// </summary>
        /// <param name="address">The address that the mail is to.</param>
        /// <returns>The RCPT command.</returns>
        public virtual SmtpCommand CreateRcpt(IMailbox address)
        {
            return new RcptCommand(address, _mailboxFilterFactory);
        }

        /// <summary>
        /// Create a DATA command.
        /// </summary>
        /// <returns>The DATA command.</returns>
        public virtual SmtpCommand CreateData()
        {
            return new DataCommand(_messageStoreFactory);
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
            return new StartTlsCommand(_options.ServerCertificate, _options.SupportedSslProtocols);
        }

        /// <summary>
        /// Create a AUTH command.
        /// </summary>
        /// <param name="method">The authentication method.</param>
        /// <param name="parameter">The authentication parameter.</param>
        /// <returns>The AUTH command.</returns>
        public SmtpCommand CreateAuth(AuthenticationMethod method, string parameter)
        {
            return new AuthCommand(method, parameter, _userAuthenticatorFactory);
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
using System.Net;
using System.Security.Cryptography.X509Certificates;
using SmtpServer.Authentication;
using SmtpServer.Storage;

namespace SmtpServer
{
    public sealed class OptionsBuilder
    {
        readonly SmtpServerOptions _options = new SmtpServerOptions();

        /// <summary>
        /// Builds the options that have been set and returns the built instance.
        /// </summary>
        /// <returns>The server options that have been set.</returns>
        public ISmtpServerOptions Build()
        {
            _options.MessageStoreFactory = _options.MessageStoreFactory ?? new DoNothingMessageStore();
            _options.CommandHandler = _options.CommandHandler ?? new DefaultCommandHandler();

            return _options;
        }

        /// <summary>
        /// Sets the server name.
        /// </summary>
        /// <param name="serverName">The name of the server.</param>
        /// <returns>A OptionsBuilder to continue building on.</returns>
        public OptionsBuilder ServerName(string serverName)
        {
            _options.ServerName = serverName;

            return this;
        }

        /// <summary>
        /// Sets the X509 certificate to use when starting a TLS session.
        /// </summary>
        /// <param name="serverCertificate">The server's certificate to use when starting a TLS session.</param>
        /// <returns>A OptionsBuilder to continue building on.</returns>
        public OptionsBuilder Certificate(X509Certificate serverCertificate)
        {
            _options.ServerCertificate = serverCertificate;

            return this;
        }

        /// <summary>
        /// Adds an endpoint to listen on.
        /// </summary>
        /// <param name="endpoint">The endpoint to listen on.</param>
        /// <returns>A OptionsBuilder to continue building on.</returns>
        public OptionsBuilder Endpoint(IPEndPoint endpoint)
        {
            _options.Endpoints.Add(endpoint);

            return this;
        }

        /// <summary>
        /// Adds an endpoint with the given port.
        /// </summary>
        /// <param name="ports">The port to add as the endpoint.</param>
        /// <returns>A OptionsBuilder to continue building on.</returns>
        public OptionsBuilder Port(params int[] ports)
        {
            foreach (var port in ports)
            {
                _options.Endpoints.Add(new IPEndPoint(IPAddress.Any, port));
            }

            return this;
        }

        /// <summary>
        /// Adds a message store factory.
        /// </summary>
        /// <param name="messageStoreFactory">The message store factory to use.</param>
        /// <returns>A OptionsBuilder to continue building on.</returns>
        public OptionsBuilder MessageStore(IMessageStoreFactory messageStoreFactory)
        {
            _options.MessageStoreFactory = messageStoreFactory;

            return this;
        }

        /// <summary>
        /// Adds a mailbox filter factory.
        /// </summary>
        /// <param name="mailboxFilterFactory">The mailbox filter factory to add.</param>
        /// <returns>A OptionsBuilder to continue building on.</returns>
        public OptionsBuilder MailboxFilter(IMailboxFilterFactory mailboxFilterFactory)
        {
            _options.MailboxFilterFactories.Add(mailboxFilterFactory);

            return this;
        }

        /// <summary>
        /// Sets the User Authenticator.
        /// </summary>
        /// <param name="userAuthenticator">The user authenticator.</param>
        /// <returns>A OptionsBuilder to continue building on.</returns>
        public OptionsBuilder UserAuthenticator(IUserAuthenticator userAuthenticator)
        {
            _options.UserAuthenticator = userAuthenticator;

            return this;
        }

        /// <summary>
        /// Sets the command handler service.
        /// </summary>
        /// <param name="commandHandler">The command handler service.</param>
        /// <returns>A OptionsBuilder to continue building on.</returns>
        public OptionsBuilder CommandHandler(ICommandHandler commandHandler)
        {
            _options.CommandHandler = commandHandler;

            return this;
        }

        /// <summary>
        /// Sets a value indicating whether authentication should be allowed on an unsecure session.
        /// </summary>
        /// <param name="value">true if the AUTH command is available on an unsecure session, false if not.</param>
        /// <returns>A OptionsBuilder to continue building on.</returns>
        public OptionsBuilder AllowUnsecureAuthentication(bool value = true)
        {
            _options.AllowUnsecureAuthentication = value;

            return this;
        }

        /// <summary>
        /// Sets the maximum message size.
        /// </summary>
        /// <param name="maxMessageSize">The maximum message size to allow.</param>
        /// <returns>A OptionsBuilder to continue building on.</returns>
        public OptionsBuilder MaxMessageSize(int maxMessageSize)
        {
            _options.MaxMessageSize = maxMessageSize;

            return this;
        }
    }
}
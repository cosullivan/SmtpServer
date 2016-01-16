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
            _options.MessageStore = _options.MessageStore ?? new DoNothingMessageStore();

            return _options;
        }

        /// <summary>
        /// Sets the server name.
        /// </summary>
        /// <param name="serverName">The name of the server.</param>
        /// <returns>A OptionsBuilder to continue building on.</returns>
        public OptionsBuilder WithServerName(string serverName)
        {
            _options.ServerName = serverName;

            return this;
        }

        /// <summary>
        /// Sets the X509 certificate to use when starting a TLS session.
        /// </summary>
        /// <param name="serverCertificate">The server's certificate to use when starting a TLS session.</param>
        /// <returns>A OptionsBuilder to continue building on.</returns>
        public OptionsBuilder WithCertificate(X509Certificate serverCertificate)
        {
            _options.ServerCertificate = serverCertificate;

            return this;
        }

        /// <summary>
        /// Adds an endpoint to listen on.
        /// </summary>
        /// <param name="endpoint">The endpoint to listen on.</param>
        /// <returns>A OptionsBuilder to continue building on.</returns>
        public OptionsBuilder WithEndpoint(IPEndPoint endpoint)
        {
            _options.Endpoints.Add(endpoint);

            return this;
        }

        /// <summary>
        /// Adds an endpoint with the given port.
        /// </summary>
        /// <param name="ports">The port to add as the endpoint.</param>
        /// <returns>A OptionsBuilder to continue building on.</returns>
        public OptionsBuilder WithPort(params int[] ports)
        {
            foreach (var port in ports)
            {
                _options.Endpoints.Add(new IPEndPoint(IPAddress.Any, port));
            }

            return this;
        }

        /// <summary>
        /// Adds a message store.
        /// </summary>
        /// <param name="messageStore">The message store to add.</param>
        /// <returns>A OptionsBuilder to continue building on.</returns>
        public OptionsBuilder WithMessageStore(IMessageStore messageStore)
        {
            _options.MessageStore = messageStore;

            return this;
        }

        /// <summary>
        /// Adds a mailbox filter.
        /// </summary>
        /// <param name="mailboxFilter">The mailbox filter to add.</param>
        /// <returns>A OptionsBuilder to continue building on.</returns>
        public OptionsBuilder WithMailboxFilter(IMailboxFilter mailboxFilter)
        {
            _options.MailboxFilters.Add(mailboxFilter);

            return this;
        }

        /// <summary>
        /// Sets the User Authenticator.
        /// </summary>
        /// <param name="userAuthenticator">The user authenticator.</param>
        /// <returns>A OptionsBuilder to continue building on.</returns>
        public OptionsBuilder WithUserAuthenticator(IUserAuthenticator userAuthenticator)
        {
            _options.UserAuthenticator = userAuthenticator;

            return this;
        }

        /// <summary>
        /// Sets the maximum message size.
        /// </summary>
        /// <param name="maxMessageSize">The maximum message size to allow.</param>
        /// <returns>A OptionsBuilder to continue building on.</returns>
        public OptionsBuilder WithMaxMessageSize(int maxMessageSize)
        {
            _options.MaxMessageSize = maxMessageSize;

            return this;
        }
    }
}

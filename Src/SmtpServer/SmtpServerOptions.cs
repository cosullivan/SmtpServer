using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using SmtpServer.Authentication;
using SmtpServer.Storage;

namespace SmtpServer
{
    internal sealed class SmtpServerOptions : ISmtpServerOptions
    {
        readonly Collection<IPEndPoint> _endpoints = new Collection<IPEndPoint>();
        readonly Collection<IMailboxFilterFactory> _mailboxFilterFactories = new Collection<IMailboxFilterFactory>();

        /// <summary>
        /// Gets or sets the maximum size of a message.
        /// </summary>
        public int MaxMessageSize { get; internal set; }

        /// <summary>
        /// The maximum number of retries before quitting the session.
        /// </summary>
        public int MaxRetryCount { get; internal set; }

        /// <summary>
        /// Gets or sets the SMTP server name.
        /// </summary>
        public string ServerName { get; internal set; }

        /// <summary>
        /// Gets the Server Certificate to use when starting a TLS session.
        /// </summary>
        public X509Certificate ServerCertificate { get; internal set; }

        /// <summary>
        /// Gets or sets the endpoint to listen on.
        /// </summary>
        internal Collection<IPEndPoint> Endpoints => _endpoints;

        /// <summary>
        /// Gets or sets the endpoint to listen on.
        /// </summary>
        IReadOnlyCollection<IPEndPoint> ISmtpServerOptions.Endpoints => new ReadOnlyCollection<IPEndPoint>(_endpoints);

        /// <summary>
        /// Gets or sets the mailbox filter factories to use.
        /// </summary>
        internal Collection<IMailboxFilterFactory> MailboxFilterFactories => _mailboxFilterFactories;

        /// <summary>
        /// Gets the message store factory to use.
        /// </summary>
        public IMessageStoreFactory MessageStoreFactory { get; internal set; }

        /// <summary>
        /// Gets the mailbox filter factory to use.
        /// </summary>
        public IMailboxFilterFactory MailboxFilterFactory
        {
            get
            {
                if (_mailboxFilterFactories.Count == 1)
                {
                    return _mailboxFilterFactories.First();
                }

                return new CompositeMailboxFilterFactory(_mailboxFilterFactories.ToArray());
            }
        }

        /// <summary>
        /// Gets the user authenticator factory to use.
        /// </summary>
        public IUserAuthenticatorFactory UserAuthenticatorFactory { get; internal set; }

        /// <summary>
        /// Gets a value indicating whether authentication should be allowed on an unsecure session.
        /// </summary>
        public bool AllowUnsecureAuthentication { get; internal set; }

        /// <summary>
        /// Gets a value indicating whether the client must authenticate in order to proceed.
        /// </summary>
        public bool AuthenticationRequired { get; internal set; }

        /// <summary>
        /// The supported SSL protocols.
        /// </summary>
        public SslProtocols SupportedSslProtocols { get; internal set; }

        /// <summary>
        /// The timeout to use when waiting for a command from the client.
        /// </summary>
        public TimeSpan CommandWaitTimeout { get; internal set; }

        /// <summary>
        /// The size of the buffer that is read from each call to the underlying network client.
        /// </summary>
        public int NetworkBufferSize { get; internal set; }

        /// <summary>
        /// The timeout on each individual buffer read.
        /// </summary>
        public TimeSpan NetworkBufferReadTimeout { get; internal set; }

        /// <summary>
        /// The logger instance to use.
        /// </summary>
        public ILogger Logger { get; internal set; }
    }
}
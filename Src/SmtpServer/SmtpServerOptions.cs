using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using SmtpServer.Authentication;
using SmtpServer.Storage;

namespace SmtpServer
{
    internal sealed class SmtpServerOptions : ISmtpServerOptions
    {
        readonly Collection<IPEndPoint> _endpoints = new Collection<IPEndPoint>();
        readonly Collection<IMailboxFilter> _mailboxFilters = new Collection<IMailboxFilter>();

        /// <summary>
        /// Gets or sets the maximum size of a message.
        /// </summary>
        public int MaxMessageSize { get; internal set; }

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
        internal Collection<IPEndPoint> Endpoints
        {
            get { return _endpoints; }
        }

        /// <summary>
        /// Gets or sets the endpoint to listen on.
        /// </summary>
        IReadOnlyCollection<IPEndPoint> ISmtpServerOptions.Endpoints
        {
            get { return new ReadOnlyCollection<IPEndPoint>(_endpoints); }
        }

        /// <summary>
        /// Gets or sets the mailbox filters to listen on.
        /// </summary>
        internal Collection<IMailboxFilter> MailboxFilters
        {
            get { return _mailboxFilters; }
        }

        /// <summary>
        /// Gets or sets the message store to use.
        /// </summary>
        public IMessageStore MessageStore { get; internal set; }

        /// <summary>
        /// Gets the mailbox filter.
        /// </summary>
        public IMailboxFilter MailboxFilter
        {
            get
            {
                if (_mailboxFilters.Count == 1)
                {
                    return _mailboxFilters.First();
                }

                return new CompositeMailboxFilter(_mailboxFilters.ToArray());
            }
        }

        /// <summary>
        /// Gets the user authenticator.
        /// </summary>
        public IUserAuthenticator UserAuthenticator { get; internal set; }

        /// <summary>
        /// Gets a value indicating whether authentication should be allowed on an unsecure session.
        /// </summary>
        public bool AllowUnsecureAuthentication { get; internal set; }
    }
}

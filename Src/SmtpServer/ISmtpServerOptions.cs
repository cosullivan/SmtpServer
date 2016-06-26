using System.Collections.Generic;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using SmtpServer.Authentication;
using SmtpServer.Storage;

namespace SmtpServer
{
    public interface ISmtpServerOptions
    {
        /// <summary>
        /// Gets the maximum size of a message.
        /// </summary>
        int MaxMessageSize { get; }

        /// <summary>
        /// Gets the SMTP server name.
        /// </summary>
        string ServerName { get; }

        /// <summary>
        /// Gets the Server Certificate to use when starting a TLS session.
        /// </summary>
        X509Certificate ServerCertificate { get; }

        /// <summary>
        /// Gets the collection of endpoints to listen on.
        /// </summary>
        IReadOnlyCollection<IPEndPoint> Endpoints { get; }

        /// <summary>
        /// Gets the message store factory to use.
        /// </summary>
        IMessageStoreFactory MessageStoreFactory { get; }

        /// <summary>
        /// Gets the mailbox filter.
        /// </summary>
        IMailboxFilter MailboxFilter { get; }

        /// <summary>
        /// Gets the user authenticator.
        /// </summary>
        IUserAuthenticator UserAuthenticator { get; }

        /// <summary>
        /// Gets a value indicating whether authentication should be allowed on an unsecure session.
        /// </summary>
        bool AllowUnsecureAuthentication { get; }
    }
}

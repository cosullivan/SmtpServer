﻿using System;
using System.Collections.Generic;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using SmtpServer.Authentication;
using SmtpServer.Net;
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
        /// The maximum number of retries before quitting the session.
        /// </summary>
        int MaxRetryCount { get; }

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
        IReadOnlyList<IEndpointDefinition> Endpoints { get; }

        /// <summary>
        /// The endpoint listener factory.
        /// </summary>
        IEndpointListenerFactory EndpointListenerFactory { get; }

        /// <summary>
        /// Gets the message store factory to use.
        /// </summary>
        IMessageStoreFactory MessageStoreFactory { get; }

        /// <summary>
        /// Gets the mailbox filter factory to use.
        /// </summary>
        IMailboxFilterFactory MailboxFilterFactory { get; }

        /// <summary>
        /// Gets the user authenticator factory to use.
        /// </summary>
        IUserAuthenticatorFactory UserAuthenticatorFactory { get; }

        /// <summary>
        /// The supported SSL protocols.
        /// </summary>
        SslProtocols SupportedSslProtocols { get; }

        /// <summary>
        /// The timeout to use when waiting for a command from the client.
        /// </summary>
        TimeSpan CommandWaitTimeout { get; }

        /// <summary>
        /// The size of the buffer that is read from each call to the underlying network client.
        /// </summary>
        int NetworkBufferSize { get; }

        /// <summary>
        /// The timeout on each individual buffer read.
        /// </summary>
        TimeSpan NetworkBufferReadTimeout { get; }

        /// <summary>
        /// The logger instance to use.
        /// </summary>
        ILogger Logger { get; }
    }
}
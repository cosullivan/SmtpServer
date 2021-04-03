using System;
using System.Collections.Generic;

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
        /// The maximum number of authentication attempts.
        /// </summary>
        int MaxAuthenticationAttempts { get; }

        /// <summary>
        /// Gets the SMTP server name.
        /// </summary>
        string ServerName { get; }

        /// <summary>
        /// Gets the collection of endpoints to listen on.
        /// </summary>
        IReadOnlyList<IEndpointDefinition> Endpoints { get; }

        /// <summary>
        /// The timeout to use when waiting for a command from the client.
        /// </summary>
        TimeSpan CommandWaitTimeout { get; }

        /// <summary>
        /// The size of the buffer that is read from each call to the underlying network client.
        /// </summary>
        int NetworkBufferSize { get; }
    }
}
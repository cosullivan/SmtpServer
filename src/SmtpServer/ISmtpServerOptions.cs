using System;
using System.Collections.Generic;

namespace SmtpServer
{
    /// <summary>
    /// Smtp Server Options Interface
    /// </summary>
    public interface ISmtpServerOptions
    {
        /// <summary>
        /// Gets the maximum message size option.
        /// </summary>
        IMaxMessageSizeOptions MaxMessageSizeOptions { get; }

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

        /// <summary>
        /// Gets the custom SMTP greeting message that the server sends immediately after a client connects,
        /// typically as the initial "220" response. The message can be dynamically generated based on the session context.
        /// If not set, a default greeting will be used (e.g., "220 mail.example.com ESMTP ready").
        /// </summary>
        Func<ISessionContext, string> CustomSmtpGreeting { get; }
    }
}

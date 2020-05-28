using System;
using System.Net;

namespace SmtpServer
{
    public interface IEndpointDefinition
    {
        /// <summary>
        /// The IP endpoint to listen on.
        /// </summary>
        IPEndPoint Endpoint { get; }

        /// <summary>
        /// Indicates whether the endpoint is secure by default.
        /// </summary>
        bool IsSecure { get; }

        /// <summary>
        /// Gets a value indicating whether the client must authenticate in order to proceed.
        /// </summary>
        bool AuthenticationRequired { get; }

        /// <summary>
        /// Gets a value indicating whether authentication should be allowed on an unsecure session.
        /// </summary>
        bool AllowUnsecureAuthentication { get; }

        /// <summary>
        /// The timeout on each individual buffer read.
        /// </summary>
        TimeSpan ReadTimeout { get; }
    }
}
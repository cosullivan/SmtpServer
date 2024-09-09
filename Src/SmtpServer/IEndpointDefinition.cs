using System;
using System.Net;
using System.Security.Authentication;

namespace SmtpServer
{
    /// <summary>
    /// Endpoint Definition Interface
    /// </summary>
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
        /// The timeout of an Smtp session.
        /// </summary>
        TimeSpan SessionTimeout { get; }

        /// <summary>
        /// Gets the Server Certificate factory to use when starting a TLS session.
        /// </summary>
        ICertificateFactory CertificateFactory { get; }

        /// <summary>
        /// The supported SSL protocols.
        /// </summary>
        SslProtocols SupportedSslProtocols { get; }
    }
}

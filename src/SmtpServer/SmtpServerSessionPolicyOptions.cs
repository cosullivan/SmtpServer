using System;
using System.Threading;
using System.Threading.Tasks;
using SmtpServer.Protocol;

namespace SmtpServer
{
    /// <summary>
    /// Defines optional SMTP session policy callbacks.
    /// </summary>
    public sealed class SmtpServerSessionPolicyOptions
    {
        /// <summary>
        /// Configures a callback that runs after a connection is accepted and before the SMTP greeting is sent.
        /// </summary>
        /// <param name="callback">The callback used to evaluate the accepted connection.</param>
        /// <returns>The current options instance.</returns>
        public SmtpServerSessionPolicyOptions OnConnectionAccepted(Func<ISessionContext, CancellationToken, Task<SmtpResponse>> callback)
        {
            ConnectionAccepted = callback ?? throw new ArgumentNullException(nameof(callback));
            return this;
        }

        /// <summary>
        /// Configures a callback that runs before accepting an EHLO or HELO identity.
        /// </summary>
        /// <param name="callback">The callback used to evaluate the supplied EHLO or HELO identity.</param>
        /// <returns>The current options instance.</returns>
        public SmtpServerSessionPolicyOptions OnHelo(Func<ISessionContext, string, CancellationToken, Task<SmtpResponse>> callback)
        {
            Helo = callback ?? throw new ArgumentNullException(nameof(callback));
            return this;
        }

        /// <summary>
        /// Gets the callback that evaluates accepted connections.
        /// </summary>
        public Func<ISessionContext, CancellationToken, Task<SmtpResponse>> ConnectionAccepted { get; private set; }

        /// <summary>
        /// Gets the callback that evaluates EHLO and HELO identities.
        /// </summary>
        public Func<ISessionContext, string, CancellationToken, Task<SmtpResponse>> Helo { get; private set; }
    }
}

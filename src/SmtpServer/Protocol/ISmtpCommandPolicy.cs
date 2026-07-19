using System.Threading;
using System.Threading.Tasks;

namespace SmtpServer.Protocol
{
    /// <summary>
    /// Provides policy responses for optional SMTP commands.
    /// </summary>
    public interface ISmtpCommandPolicy
    {
        /// <summary>
        /// Gets the HELP response.
        /// </summary>
        /// <param name="context">The session context.</param>
        /// <param name="argument">The optional command argument.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The SMTP response.</returns>
        Task<SmtpResponse> GetHelpAsync(ISessionContext context, string argument, CancellationToken cancellationToken);

        /// <summary>
        /// Gets the VRFY response.
        /// </summary>
        /// <param name="context">The session context.</param>
        /// <param name="argument">The verification argument.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The SMTP response.</returns>
        Task<SmtpResponse> VerifyAsync(ISessionContext context, string argument, CancellationToken cancellationToken);

        /// <summary>
        /// Gets the EXPN response.
        /// </summary>
        /// <param name="context">The session context.</param>
        /// <param name="argument">The expansion argument.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The SMTP response.</returns>
        Task<SmtpResponse> ExpandAsync(ISessionContext context, string argument, CancellationToken cancellationToken);
    }
}

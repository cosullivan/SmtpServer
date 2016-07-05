using System.Threading;
using System.Threading.Tasks;
using SmtpServer.Mail;
using SmtpServer.Protocol;

namespace SmtpServer.Storage
{
    public interface IMessageStore
    {
        /// <summary>
        /// Save the given message to the underlying storage system.
        /// </summary>
        /// <param name="context">The session level context.</param>
        /// <param name="message">The SMTP message to store.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The response code to return that indicates the result of the message being saved.</returns>
        Task<SmtpResponse> SaveAsync(ISessionContext context, IMimeMessage message, CancellationToken cancellationToken);
    }
}
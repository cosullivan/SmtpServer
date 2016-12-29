using System.Threading;
using System.Threading.Tasks;

namespace SmtpServer
{
    public interface ISmtpCommandPipeline
    {
        /// <summary>
        /// Execute the command handler against the specified session context.
        /// </summary>
        /// <param name="context">The session context to execute the command handler against.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task which asynchronously performs the execution.</returns>
        Task ExecuteAsync(ISmtpSessionContext context, CancellationToken cancellationToken);
    }
}
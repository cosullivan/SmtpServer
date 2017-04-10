using System.Threading;
using System.Threading.Tasks;

namespace SmtpServer.Protocol
{
    public abstract class SmtpCommand
    {
        /// <summary>
        /// Execute the command.
        /// </summary>
        /// <param name="context">The execution context to operate on.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task which asynchronously performs the execution.</returns>
        internal abstract Task ExecuteAsync(ISmtpSessionContext context, CancellationToken cancellationToken);
    }
}
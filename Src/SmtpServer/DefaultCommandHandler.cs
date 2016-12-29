using System.Threading;
using System.Threading.Tasks;
using SmtpServer.Protocol;

namespace SmtpServer
{
    internal sealed class DefaultCommandHandler : ICommandHandler
    {
        /// <summary>
        /// Execute the command.
        /// </summary>
        /// <param name="command">The command to execute.</param>
        /// <param name="context">The execution context to operate on.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task which asynchronously performs the execution.</returns>
        public Task ExecuteAsync(SmtpCommand command, ISmtpSessionContext context, CancellationToken cancellationToken)
        {
            return command.ExecuteAsync(context, cancellationToken);
        }
    }
}
using System.Threading;
using System.Threading.Tasks;

namespace SmtpServer.Protocol
{
    public sealed class NoopCommand : SmtpCommand
    {
        public static readonly SmtpCommand Instance = new NoopCommand();

        /// <summary>
        /// Execute the command.
        /// </summary>
        /// <param name="context">The execution context to operate on.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task which asynchronously performs the execution.</returns>
        public override Task ExecuteAsync(ISmtpSessionContext context, CancellationToken cancellationToken)
        {
            return context.Text.ReplyAsync(SmtpResponse.Ok, cancellationToken);
        }
    }
}

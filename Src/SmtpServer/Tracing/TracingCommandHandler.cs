using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using SmtpServer.Protocol;

namespace SmtpServer.Tracing
{
    public sealed class TracingCommandHandler : ICommandHandler
    {
        readonly SmtpCommandVisitor _visitor;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="output">The output stream to write the command execution to.</param>
        public TracingCommandHandler(TextWriter output)
        {
            if (output == null)
            {
                throw new ArgumentException(nameof(output));
            }

            _visitor = new TracingSmtpCommandVisitor(output);
        }

        /// <summary>
        /// Execute the command.
        /// </summary>
        /// <param name="command">The command to execute.</param>
        /// <param name="context">The execution context to operate on.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task which asynchronously performs the execution.</returns>
        public Task ExecuteAsync(SmtpCommand command, ISmtpSessionContext context, CancellationToken cancellationToken)
        {
            _visitor.Visit(command);

            return command.ExecuteAsync(context, cancellationToken);
        }
    }
}
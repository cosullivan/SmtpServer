using System.Threading;
using System.Threading.Tasks;
using SmtpServer.IO;

namespace SmtpServer.Protocol
{
    public sealed class HeloCommand : SmtpCommand
    {
        public const string Command = "HELO";

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="options">The server options.</param>
        /// <param name="domain">The domain name.</param>
        internal HeloCommand(ISmtpServerOptions options, string domain) : base(options)
        {
            Domain = domain;
        }

        /// <summary>
        /// Execute the command.
        /// </summary>
        /// <param name="context">The execution context to operate on.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task which asynchronously performs the execution.</returns>
        internal override Task ExecuteAsync(SmtpSessionContext context, CancellationToken cancellationToken)
        {
            var response = new SmtpResponse(SmtpReplyCode.Ok, $"Hello {Domain}, haven't we met before?");

            return context.Client.ReplyAsync(response, cancellationToken);
        }

        /// <summary>
        /// Gets the domain name.
        /// </summary>
        public string Domain { get; }
    }
}
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
        /// <returns>Returns true if the command executed successfully such that the transition to the next state should occurr, false 
        /// if the current state is to be maintained.</returns>
        internal override async Task<bool> ExecuteAsync(SmtpSessionContext context, CancellationToken cancellationToken)
        {
            var response = new SmtpResponse(SmtpReplyCode.Ok, $"Hello {Domain}, haven't we met before?");

            await context.NetworkClient.ReplyAsync(response, cancellationToken).ReturnOnAnyThread();

            return true;
        }

        /// <summary>
        /// Gets the domain name.
        /// </summary>
        public string Domain { get; }
    }
}
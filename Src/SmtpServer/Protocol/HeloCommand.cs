using System.Threading;
using System.Threading.Tasks;

namespace SmtpServer.Protocol
{
    public sealed class HeloCommand : SmtpCommand
    {
        readonly string _domain;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="domain">The domain name.</param>
        public HeloCommand(string domain)
        {
            _domain = domain;
        }

        /// <summary>
        /// Execute the command.
        /// </summary>
        /// <param name="context">The execution context to operate on.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task which asynchronously performs the execution.</returns>
        public override Task ExecuteAsync(ISmtpSessionContext context, CancellationToken cancellationToken)
        {
            var response = new SmtpResponse(SmtpReplyCode.Ok, $"Hello {Domain}, haven't we met before?");

            return context.Text.ReplyAsync(response, cancellationToken);
        }

        /// <summary>
        /// Gets the domain name.
        /// </summary>
        public string Domain
        {
            get { return _domain; }
        }
    }
}

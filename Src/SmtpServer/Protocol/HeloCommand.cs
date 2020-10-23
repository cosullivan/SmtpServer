using System.Threading;
using System.Threading.Tasks;
using SmtpServer.IO;

namespace SmtpServer.Protocol
{
    public sealed class HeloCommand : SmtpCommand
    {
        public const string Command = "HELO";

        readonly string _greeting;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="domainOrAddress">The domain name.</param>
        /// <param name="greeting">The greeting text.</param>
        public HeloCommand(string domainOrAddress, string greeting) : base(Command)
        {
            DomainOrAddress = domainOrAddress;

            _greeting = greeting;
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
            var response = new SmtpResponse(SmtpReplyCode.Ok, _greeting);

            await context.Pipe.Output.WriteReplyAsync(response, cancellationToken).ConfigureAwait(false);

            return true;
        }

        /// <summary>
        /// Gets the domain name.
        /// </summary>
        public string DomainOrAddress { get; }
    }
}
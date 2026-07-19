using System.Threading;
using System.Threading.Tasks;
using SmtpServer.IO;

namespace SmtpServer.Protocol
{
    /// <summary>
    /// Helo Command
    /// </summary>
    public class HeloCommand : SmtpCommand
    {
        /// <summary>
        /// Smtp Helo Command
        /// </summary>
        public const string Command = "HELO";

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="domainOrAddress">The domain name.</param>
        public HeloCommand(string domainOrAddress) : base(Command)
        {
            DomainOrAddress = domainOrAddress;
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
            if (await AcceptHeloAsync(context, cancellationToken).ConfigureAwait(false) == false)
            {
                return false;
            }

            var response = new SmtpResponse(SmtpReplyCode.Ok, GetGreeting(context));

            await context.Pipe.Output.WriteReplyAsync(response, cancellationToken).ConfigureAwait(false);

            return true;
        }

        /// <summary>
        /// Returns the greeting to display to the remote host.
        /// </summary>
        /// <param name="context">The session context.</param>
        /// <returns>The greeting text to display to the remote host.</returns>
        protected virtual string GetGreeting(ISessionContext context)
        {
            return $"{context.ServerOptions.ServerName} Hello {DomainOrAddress}, haven't we met before?";
        }

        async Task<bool> AcceptHeloAsync(SmtpSessionContext context, CancellationToken cancellationToken)
        {
            var policy = context.ServerOptions.SessionPolicy;
            if (policy.Helo == null)
            {
                return true;
            }

            var response = await policy.Helo(context, DomainOrAddress, cancellationToken).ConfigureAwait(false);
            if (SmtpSession.IsSuccessResponse(response))
            {
                return true;
            }

            await context.Pipe.Output.WriteReplyAsync(response, cancellationToken).ConfigureAwait(false);
            return false;
        }

        /// <summary>
        /// Gets the domain name.
        /// </summary>
        public string DomainOrAddress { get; }
    }
}

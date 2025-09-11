using System.Threading;
using System.Threading.Tasks;

namespace SmtpServer.Protocol
{
    /// <summary>
    /// Smtp Command
    /// </summary>
    public abstract class SmtpCommand
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="name">The name of the command.</param>
        protected SmtpCommand(string name)
        {
            Name = name;
        }

        /// <summary>
        /// Execute the command.
        /// </summary>
        /// <param name="context">The execution context to operate on.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Returns true if the command executed successfully such that the transition to the next state should occurr, false 
        /// if the current state is to be maintained.</returns>
        internal abstract Task<bool> ExecuteAsync(SmtpSessionContext context, CancellationToken cancellationToken);

        /// <summary>
        /// The name of the command.
        /// </summary>
        public string Name { get; }
    }
}

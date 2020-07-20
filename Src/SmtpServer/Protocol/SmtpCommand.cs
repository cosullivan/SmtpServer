using System;
using System.Threading;
using System.Threading.Tasks;

namespace SmtpServer.Protocol
{
    public abstract class SmtpCommand
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="name">The name of the command.</param>
        /// <param name="options">The server options.</param>
        protected SmtpCommand(string name, ISmtpServerOptions options)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            Name = name;
            Options = options;
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
        /// The options that the command can utilise.
        /// </summary>
        protected ISmtpServerOptions Options { get; }

        /// <summary>
        /// The name of the command.
        /// </summary>
        public string Name { get; }
    }
}
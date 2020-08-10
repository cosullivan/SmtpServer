using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SmtpServer.IO;

namespace SmtpServer.Protocol
{
    public sealed class EhloCommand : SmtpCommand
    {
        public const string Command = "EHLO";

        readonly string _greeting;
        readonly Func<ISessionContext, IEnumerable<string>> _extensionsFactory;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="domainOrAddress">The domain name or address literal.</param>
        /// <param name="greeting">The greeting text.</param>
        /// <param name="extensionsFactory">The factory method that returns the list of available extensions for the current session.</param>
        internal EhloCommand(string domainOrAddress, string greeting, Func<ISessionContext, IEnumerable<string>> extensionsFactory) : base(Command)
        {
            DomainOrAddress = domainOrAddress;

            _greeting = greeting;
            _extensionsFactory = extensionsFactory;
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
            var output = new[] { _greeting }.Union(_extensionsFactory(context)).ToArray();

            for (var i = 0; i < output.Length - 1; i++)
            {
                context.Pipe.Output.WriteLine($"250-{output[i]}");
            }

            context.Pipe.Output.WriteLine($"250 {output[output.Length - 1]}");

            await context.Pipe.Output.FlushAsync(cancellationToken).ConfigureAwait(false);

            return true;
        }

        /// <summary>
        /// Gets the domain name or address literal.
        /// </summary>
        public string DomainOrAddress { get; }
    }
}
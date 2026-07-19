using System.Threading;
using System.Threading.Tasks;
using SmtpServer.ComponentModel;
using SmtpServer.IO;

namespace SmtpServer.Protocol
{
    /// <summary>
    /// Help Command
    /// </summary>
    public sealed class HelpCommand : SmtpCommand
    {
        /// <summary>
        /// Smtp Help Command
        /// </summary>
        public const string Command = "HELP";

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="argument">The optional command argument.</param>
        public HelpCommand(string argument) : base(Command)
        {
            Argument = argument;
        }

        /// <inheritdoc />
        internal override async Task<bool> ExecuteAsync(SmtpSessionContext context, CancellationToken cancellationToken)
        {
            var policy = context.ServiceProvider.GetService<ISmtpCommandPolicyFactory, ISmtpCommandPolicy>(context, SmtpCommandPolicy.Default);

            using var container = new DisposableContainer<ISmtpCommandPolicy>(policy);

            var response = await container.Instance.GetHelpAsync(context, Argument, cancellationToken).ConfigureAwait(false);
            await context.Pipe.Output.WriteReplyAsync(response, cancellationToken).ConfigureAwait(false);

            return true;
        }

        /// <summary>
        /// Gets the optional command argument.
        /// </summary>
        public string Argument { get; }
    }
}

using System.Threading;
using System.Threading.Tasks;
using SmtpServer.ComponentModel;
using SmtpServer.IO;

namespace SmtpServer.Protocol
{
    /// <summary>
    /// Vrfy Command
    /// </summary>
    public sealed class VrfyCommand : SmtpCommand
    {
        /// <summary>
        /// Smtp Vrfy Command
        /// </summary>
        public const string Command = "VRFY";

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="argument">The verification argument.</param>
        public VrfyCommand(string argument) : base(Command)
        {
            Argument = argument;
        }

        /// <inheritdoc />
        internal override async Task<bool> ExecuteAsync(SmtpSessionContext context, CancellationToken cancellationToken)
        {
            var policy = context.ServiceProvider.GetService<ISmtpCommandPolicyFactory, ISmtpCommandPolicy>(context, SmtpCommandPolicy.Default);

            using var container = new DisposableContainer<ISmtpCommandPolicy>(policy);

            var response = await container.Instance.VerifyAsync(context, Argument, cancellationToken).ConfigureAwait(false);
            await context.Pipe.Output.WriteReplyAsync(response, cancellationToken).ConfigureAwait(false);

            return true;
        }

        /// <summary>
        /// Gets the verification argument.
        /// </summary>
        public string Argument { get; }
    }
}

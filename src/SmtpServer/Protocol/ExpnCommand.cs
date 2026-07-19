using System.Threading;
using System.Threading.Tasks;
using SmtpServer.ComponentModel;
using SmtpServer.IO;

namespace SmtpServer.Protocol
{
    /// <summary>
    /// Expn Command
    /// </summary>
    public sealed class ExpnCommand : SmtpCommand
    {
        /// <summary>
        /// Smtp Expn Command
        /// </summary>
        public const string Command = "EXPN";

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="argument">The expansion argument.</param>
        public ExpnCommand(string argument) : base(Command)
        {
            Argument = argument;
        }

        /// <inheritdoc />
        internal override async Task<bool> ExecuteAsync(SmtpSessionContext context, CancellationToken cancellationToken)
        {
            var policy = context.ServiceProvider.GetService<ISmtpCommandPolicyFactory, ISmtpCommandPolicy>(context, SmtpCommandPolicy.Default);

            using var container = new DisposableContainer<ISmtpCommandPolicy>(policy);

            var response = await container.Instance.ExpandAsync(context, Argument, cancellationToken).ConfigureAwait(false);
            await context.Pipe.Output.WriteReplyAsync(response, cancellationToken).ConfigureAwait(false);

            return true;
        }

        /// <summary>
        /// Gets the expansion argument.
        /// </summary>
        public string Argument { get; }
    }
}

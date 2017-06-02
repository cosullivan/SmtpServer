using System.Threading;
using System.Threading.Tasks;

namespace SmtpServer.Protocol
{
    public sealed class BdatCommand : SmtpCommand
    {
        public const string Command = "BDAT";

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="options">The server options.</param>
        internal BdatCommand(ISmtpServerOptions options, int chunkSize, bool last) : base(options)
        {
            ChunkSize = chunkSize;
            IsLast = last;
        }

        /// <summary>
        /// Execute the command.
        /// </summary>
        /// <param name="context">The execution context to operate on.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task which asynchronously performs the execution.</returns>
        internal override async Task ExecuteAsync(SmtpSessionContext context, CancellationToken cancellationToken)
        {
            await Task.Yield();
        }

        /// <summary>
        /// The chunk size.
        /// </summary>
        public int ChunkSize { get; }

        /// <summary>
        /// Returns a value indicating whether this is the last chunk.
        /// </summary>
        public bool IsLast { get; }
    }
}
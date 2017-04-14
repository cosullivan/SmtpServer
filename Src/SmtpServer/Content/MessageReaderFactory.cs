using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace SmtpServer.Content
{
    public sealed class MessageReaderFactory
    {
        /// <summary>
        /// Create an instance of the appropriate message reader for the current stream.
        /// </summary>
        /// <param name="stream">The stream to read the message from.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The message reader for the given stream.</returns>
        public Task<IMessageReader> CreateInstanceAsync(Stream stream, CancellationToken cancellationToken = default(CancellationToken))
        {
            IMessageReader reader = new TextMessageReader(stream);

            return Task.FromResult(reader);
        }
    }
}
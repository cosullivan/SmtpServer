using System.Threading;
using System.Threading.Tasks;

namespace SmtpServer.Content
{
    public interface IMessageReader
    {
        /// <summary>
        /// Read a message 
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The message that was read from the stream.</returns>
        Task<IMessage> ReadAsync(CancellationToken cancellationToken = default(CancellationToken));
    }
}
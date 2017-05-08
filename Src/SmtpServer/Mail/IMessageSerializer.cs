using System.IO;
using System.Threading;
using System.Threading.Tasks;
using SmtpServer.IO;

namespace SmtpServer.Mail
{
    public interface IMessageSerializer
    {
        /// <summary>
        /// Serialize the message to a stream.
        /// </summary>
        /// <param name="message">The message to serialize.</param>
        /// <param name="stream">The stream to serialize the message to.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The stream that the message was serialized to.</returns>
        Task SerializeAsync(IMessage message, Stream stream, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Deserialize a message from the stream.
        /// </summary>
        /// <param name="networkClient">The network client to deserialize the message from.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The message that was deserialized.</returns>
        Task<IMessage> DeserializeAsync(INetworkClient networkClient, CancellationToken cancellationToken = default(CancellationToken));
    }
}
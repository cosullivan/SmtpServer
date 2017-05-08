using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using SmtpServer.IO;

namespace SmtpServer.Mail
{
    internal sealed class TextMessageSerializer : IMessageSerializer
    {
        /// <summary>
        /// Serialize the message to a stream.
        /// </summary>
        /// <param name="message">The message to serialize.</param>
        /// <param name="stream">The stream to serialize the message to.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The stream that the message was serialized to.</returns>
        public Task SerializeAsync(IMessage message, Stream stream, CancellationToken cancellationToken = default(CancellationToken))
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Deserialize a message from the stream.
        /// </summary>
        /// <param name="networkClient">The network client to deserialize the message from.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The message that was deserialized.</returns>
        public async Task<IMessage> DeserializeAsync(INetworkClient networkClient, CancellationToken cancellationToken = default(CancellationToken))
        {
            var stream = new ByteArrayStream(await networkClient.ReadDotBlockAsync(cancellationToken));

            return new TextMessage(stream);
        }
    }
}
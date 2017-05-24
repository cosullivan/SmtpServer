using System.Threading;
using System.Threading.Tasks;
using SmtpServer.IO;
using SmtpServer.Mime;
using SmtpServer.Text;

namespace SmtpServer.Mail
{
    public sealed class MimeMessageSerializer : IMessageSerializer
    {
        /// <summary>
        /// Deserialize a message from the stream.
        /// </summary>
        /// <param name="networkClient">The network client to deserialize the message from.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The message that was deserialized.</returns>
        public async Task<IMessage> DeserializeAsync(INetworkClient networkClient, CancellationToken cancellationToken = default(CancellationToken))
        {
           var segments = await networkClient.ReadDotBlockAsync(cancellationToken);

            try
            {
                var parser = new MimeParser(new TokenEnumerator(new ByteArrayTokenReader(segments)));

                if (parser.TryMakeDocument(out MimeDocument document) == false) 
                {
                    return new TextMessage(new ByteArrayStream(segments));
                }

                return new MimeMessage(document);
            }
            catch
            {
                // fallback to a standard text message
                return new TextMessage(new ByteArrayStream(segments));
            }
        }
    }
}
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SmtpServer.Content
{
    public sealed class MimeMessageReader : IMessageReader
    {
        readonly Stream _stream;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="stream">The stream that is being read from.</param>
        public MimeMessageReader(Stream stream)
        {
            _stream = stream;
        }

        /// <summary>
        /// Read a message 
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The message that was read from the stream.</returns>
        public async Task<IMessage> ReadAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            var reader = new StreamReader(_stream, Encoding.UTF8);

            HERE: IMimeMessage should have a Message Headers and then the content/body

            Stream stream = null;

            return new MimeMessage(1.0m, stream);
        }
    }
}
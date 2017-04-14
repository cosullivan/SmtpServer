using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SmtpServer.Content
{
    internal sealed class TextMessageReader : IMessageReader
    {
        readonly Stream _stream;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="stream">The stream that is being read from.</param>
        public TextMessageReader(Stream stream)
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
            var reader = new StreamReader(_stream, Encoding.ASCII);

            var stream = await ReceiveShortLineContentAsync(reader, cancellationToken).ConfigureAwait(false);
            stream.Position = 0;

            return new TextMessage(stream);
        }

        /// <summary>
        /// Receive the message content in short line format.
        /// </summary>
        /// <param name="reader">The stream reader to read the message content from.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task which asynchronously performs the operation.</returns>
        async Task<Stream> ReceiveShortLineContentAsync(StreamReader reader, CancellationToken cancellationToken)
        {
            var writer = new StreamWriter(new MemoryStream(), Encoding.ASCII);

            try
            {
                string text;
                while ((text = await reader.ReadLineAsync(TimeSpan.FromSeconds(60), cancellationToken).ConfigureAwait(false)) != ".")
                {
                    // need to trim the '.' at the start of the line if it 
                    // exists as this would have been added for transparency
                    // http://tools.ietf.org/html/rfc5321#section-4.5.2
                    writer.WriteLine(text.TrimStart('.'));
                    writer.Flush();
                }
            }
            catch (TimeoutException)
            {
                // TODO: not sure what the best thing to do here is
                throw;
            }

            return writer.BaseStream;
        }
    }
}
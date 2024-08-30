using System.Buffers;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using SmtpServer;
using SmtpServer.Protocol;
using SmtpServer.Storage;

namespace SampleApp
{
    public class SampleMessageStore : MessageStore
    {
        readonly TextWriter _writer;

        public SampleMessageStore(TextWriter writer)
        {
            _writer = writer;
        }

        /// <summary>
        /// Save the given message to the underlying storage system.
        /// </summary>
        /// <param name="context">The session context.</param>
        /// <param name="transaction">The SMTP message transaction to store.</param>
        /// <param name="buffer">The buffer that contains the message content.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A unique identifier that represents this message in the underlying message store.</returns>
        public override async Task<SmtpResponse> SaveAsync(ISessionContext context, IMessageTransaction transaction, ReadOnlySequence<byte> buffer, CancellationToken cancellationToken)
        {
            await using var stream = new MemoryStream();

            var position = buffer.GetPosition(0);
            while (buffer.TryGet(ref position, out var memory))
            {
                stream.Write(memory.Span);
            }

            stream.Position = 0;

            var message = await MimeKit.MimeMessage.LoadAsync(stream, cancellationToken);

            _writer.WriteLine("Subject={0}", message.Subject);
            _writer.WriteLine("Body={0}", message.Body);

            return SmtpResponse.Ok;
        }
    }
}

using SmtpServer.Mime;

namespace SmtpServer.Mail
{
    internal sealed class MimeMessage : IMimeMessage
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="document">The MIME document.</param>
        public MimeMessage(MimeDocument document)
        {
            Document = document;
        }

        /// <summary>
        /// Returns the message type.
        /// </summary>
        public MessageType MessageType => MessageType.Mime;

        /// <summary>
        /// The MIME message document.
        /// </summary>
        public MimeDocument Document { get; }
    }
}
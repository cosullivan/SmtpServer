using System.IO;

namespace SmtpServer.Mail
{
    internal sealed class TextMessage : ITextMessage
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="content">The content for the message.</param>
        public TextMessage(Stream content)
        {
            Content = content;
        }

        /// <summary>
        /// Returns the message type.
        /// </summary>
        public MessageType MessageType => MessageType.Text;

        /// <summary>
        /// The message content.
        /// </summary>
        public Stream Content { get; }
    }
}
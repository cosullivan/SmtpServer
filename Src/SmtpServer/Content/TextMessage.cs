using System.IO;

namespace SmtpServer.Content
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
        /// The message type.
        /// </summary>
        public MessageType Type => MessageType.Text;

        /// <summary>
        /// The message content.
        /// </summary>
        public Stream Content { get; }
    }
}
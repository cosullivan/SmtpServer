using System.IO;

namespace SmtpServer.Content
{
    internal sealed class PlainMessage : IPlainMessage
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="content">The content for the message.</param>
        public PlainMessage(Stream content)
        {
            Content = content;
        }

        /// <summary>
        /// The message type.
        /// </summary>
        public MessageType Type
        {
            get { return MessageType.Plain; }
        }

        /// <summary>
        /// The message content.
        /// </summary>
        public Stream Content { get; set; }
    }
}
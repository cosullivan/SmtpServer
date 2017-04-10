using System.IO;

namespace SmtpServer.Content
{
    public sealed class MimeMessage : IMimeMessage
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="version">The MIME version.</param>
        /// <param name="content">The MIME content.</param>
        public MimeMessage(decimal version, Stream content)
        {
            Version = version;
            Content = content;
        }

        /// <summary>
        /// The MIME message version.
        /// </summary>
        public decimal Version { get; }

        /// <summary>
        /// The message type.
        /// </summary>
        public MessageType Type
        {
            get { return MessageType.Mime; }
        }

        /// <summary>
        /// The message content.
        /// </summary>
        public Stream Content { get; set; }
    }
}
using SmtpServer.Content;

namespace SmtpServer.Mime
{
    public sealed class MimeMessage : IMimeMessage
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="version">The MIME version.</param>
        /// <param name="body">The body of the message.</param>
        public MimeMessage(MimeVersion version, MimeEntity body)
        {
            Version = version;
            Body = body;
        }

        /// <summary>
        /// The message type.
        /// </summary>
        MessageType IMessage.Type => MessageType.Mime;

        /// <summary>
        /// The MIME version.
        /// </summary>
        public MimeVersion Version { get; }

        /// <summary>
        /// The message body.
        /// </summary>
        public MimeEntity Body { get; }
    }
}
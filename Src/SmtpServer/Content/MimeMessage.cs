namespace SmtpServer.Content
{
    public sealed class MimeMessage : IMimeMessage
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="version">The MIME version.</param>
        public MimeMessage(decimal version)
        {
            Version = version;
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
    }
}
namespace SmtpServer.Mime
{
    public sealed class MimeDocument
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="version">The MIME version.</param>
        /// <param name="body">The body of the message.</param>
        public MimeDocument(MimeVersion version, MimeEntity body)
        {
            Version = version;
            Body = body;
        }

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
namespace SmtpServer.Content
{
    public enum MessageType
    {
        /// <summary>
        /// A plain ASCII message.
        /// </summary>
        Plain,

        /// <summary>
        /// A MIME encoded message.
        /// </summary>
        Mime
    }
}
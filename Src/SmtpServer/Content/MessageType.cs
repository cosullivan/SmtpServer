namespace SmtpServer.Content
{
    public enum MessageType
    {
        /// <summary>
        /// A plain text ASCII message (adhering to the 822 RFC).
        /// </summary>
        Text,

        /// <summary>
        /// A MIME encoded message (adhering to the 2045 RFC).
        /// </summary>
        Mime
    }
}
namespace SmtpServer.Content
{
    public interface IMessage
    {
        /// <summary>
        /// The message type.
        /// </summary>
        MessageType Type { get; }
    }
}
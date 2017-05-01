namespace SmtpServer.Mail
{
    public interface IMessage
    {
        /// <summary>
        /// The message type.
        /// </summary>
        MessageType Type { get; }
    }
}
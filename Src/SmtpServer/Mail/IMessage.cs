namespace SmtpServer.Mail
{
    public interface IMessage
    {
        /// <summary>
        /// Returns the message type.
        /// </summary>
        MessageType MessageType { get; }
    }
}
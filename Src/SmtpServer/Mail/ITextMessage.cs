using System.IO;

namespace SmtpServer.Mail
{
    public interface ITextMessage : IMessage
    {
        /// <summary>
        /// The message content.
        /// </summary>
        Stream Content { get; }
    }
}
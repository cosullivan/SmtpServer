using System.IO;

namespace SmtpServer.Content
{
    public interface ITextMessage : IMessage
    {
        /// <summary>
        /// The message content.
        /// </summary>
        Stream Content { get; }
    }
}
using System.IO;

namespace SmtpServer.Content
{
    public interface IMessage
    {
        /// <summary>
        /// The message type.
        /// </summary>
        MessageType Type { get; }

        /// <summary>
        /// The message content.
        /// </summary>
        Stream Content { get; }
    }
}
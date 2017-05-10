using System;
using System.IO;
using System.Text;

namespace SmtpServer.Mail
{
    public interface ITextMessage : IMessage
    {
        /// <summary>
        /// The message content.
        /// </summary>
        Stream Content { get; }
    }

    public static class TextMessageExtensions
    {
        public static string ToString(this ITextMessage message, Encoding encoding)
        {
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            TODO:
        }
    }
}
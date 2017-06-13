using System;
using SmtpServer.Mail;

namespace SmtpServer.Tests
{
    internal static class MessageTransactionExtensions
    {
        /// <summary>
        /// Returns the subject from the message.
        /// </summary>
        /// <param name="messageTransaction">The message transaction to return the message subject from.</param>
        /// <returns>The message subject from the message transaction.</returns>
        public static string Subject(this IMessageTransaction messageTransaction)
        {
            if (messageTransaction == null)
            {
                throw new ArgumentNullException(nameof(messageTransaction));
            }

            var textMessage = (ITextMessage)messageTransaction.Message;

            return MimeKit.MimeMessage.Load(textMessage.Content).Subject;
        }

        /// <summary>
        /// Returns the text message body.
        /// </summary>
        /// <param name="messageTransaction">The message transaction to return the message text body from.</param>
        /// <returns>The message text body from the message transaction.</returns>
        public static string Text(this IMessageTransaction messageTransaction)
        {
            if (messageTransaction == null)
            {
                throw new ArgumentNullException(nameof(messageTransaction));
            }

            var textMessage = (ITextMessage) messageTransaction.Message;

            return MimeKit.MimeMessage.Load(textMessage.Content).TextBody;
        }
    }
}
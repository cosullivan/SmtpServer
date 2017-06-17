using System;
using System.IO;
using MimeKit;
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
            textMessage.Content.Position = 0;

            return MimeKit.MimeMessage.Load(textMessage.Content).Subject;
        }

        /// <summary>
        /// Returns the text message body.
        /// </summary>
        /// <param name="messageTransaction">The message transaction to return the message text body from.</param>
        /// <param name="charset">The character set to use.</param>
        /// <returns>The message text body from the message transaction.</returns>
        public static string Text(this IMessageTransaction messageTransaction, string charset = "utf-8")
        {
            if (messageTransaction == null)
            {
                throw new ArgumentNullException(nameof(messageTransaction));
            }

            var textMessage = (ITextMessage) messageTransaction.Message;
            textMessage.Content.Position = 0;

            var message = MimeKit.MimeMessage.Load(textMessage.Content);

            return ((TextPart) message.Body).GetText(charset).TrimEnd('\n', '\r');
        }

        /// <summary>
        /// Return the MIME content of the text message.
        /// </summary>
        /// <param name="messageTransaction">The message transaction to return the message text body from.</param>
        /// <returns>The MIME content of the text message.</returns>
        public static string Mime(this IMessageTransaction messageTransaction)
        {
            if (messageTransaction == null)
            {
                throw new ArgumentNullException(nameof(messageTransaction));
            }

            var textMessage = (ITextMessage)messageTransaction.Message;

            return textMessage.Mime();
        }

        /// <summary>
        /// Return the MIME content of the text message.
        /// </summary>
        /// <param name="textMessage">The text message to return the MIME content for.</param>
        /// <returns>The MIME content of the text message.</returns>
        public static string Mime(this ITextMessage textMessage)
        {
            if (textMessage == null)
            {
                throw new ArgumentNullException(nameof(textMessage));
            }

            textMessage.Content.Position = 0;

            using (var reader = new StreamReader(textMessage.Content))
            {
                return reader.ReadToEnd();
            }
        }
    }
}
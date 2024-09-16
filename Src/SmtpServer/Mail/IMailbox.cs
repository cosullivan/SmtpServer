using System;
using System.Globalization;

namespace SmtpServer.Mail
{
    /// <summary>
    /// Mailbox Interface
    /// </summary>
    public interface IMailbox
    {
        /// <summary>
        /// Gets the user/account name.
        /// </summary>
        string User { get; }

        /// <summary>
        /// Gets the host server.
        /// </summary>
        string Host { get; }
    }

    /// <summary>
    /// Mailbox Extension Methods
    /// </summary>
    public static class MailboxExtensionMethods
    {
        /// <summary>
        /// Returns the Mailbox as an Address string.
        /// </summary>
        /// <param name="mailbox">The mailbox to perform the operation on.</param>
        /// <returns>The address string that represents the mailbox.</returns>
        public static string AsAddress(this IMailbox mailbox)
        {
            if (mailbox == null)
            {
                throw new ArgumentNullException(nameof(mailbox));
            }

            if (string.IsNullOrWhiteSpace(mailbox.User) && string.IsNullOrWhiteSpace(mailbox.Host))
            {
                return null;
            }

            return string.Format(CultureInfo.InvariantCulture, "{0}@{1}", mailbox.User, mailbox.Host);
        }
    }
}

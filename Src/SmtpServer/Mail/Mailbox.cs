using System;

namespace SmtpServer.Mail
{
    public class Mailbox : IMailbox
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="user">The user/account name.</param>
        /// <param name="host">The host server.</param>
        /// <param name="displayName">The display name.</param>
        public Mailbox(string user, string host, string displayName = null)
        {
            User = user;
            Host = host;
            DisplayName = displayName;
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="address">The email address to create the mailbox from.</param>
        public Mailbox(string address)
        {
            address = address.Replace(" ", String.Empty);

            var index = address.IndexOf('@');

            User = address.Substring(0, index);
            Host = address.Substring(index + 1);
        }

        /// <summary>
        /// Gets the user/account name.
        /// </summary>
        public string User { get; }

        /// <summary>
        /// Gets the host server.
        /// </summary>
        public string Host { get; }

        /// <summary>
        /// Gets the display name.
        /// </summary>
        public string DisplayName { get; }
    }
}
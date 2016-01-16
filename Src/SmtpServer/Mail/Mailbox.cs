using System;

namespace SmtpServer.Mail
{
    public class Mailbox : IMailbox
    {
        readonly string _user;
        readonly string _host;
        readonly string _displayName;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="user">The user/account name.</param>
        /// <param name="host">The host server.</param>
        /// <param name="displayName">The display name.</param>
        public Mailbox(string user, string host, string displayName = null)
        {
            _user = user;
            _host = host;
            _displayName = displayName;
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="address">The email address to create the mailbox from.</param>
        public Mailbox(string address)
        {
            address = address.Replace(" ", String.Empty);

            var index = address.IndexOf('@');

            _user = address.Substring(0, index);
            _host = address.Substring(index + 1);
        }

        /// <summary>
        /// Gets the user/account name.
        /// </summary>
        public string User
        {
            get { return _user; }
        }

        /// <summary>
        /// Gets the host server.
        /// </summary>
        public string Host
        {
            get { return _host; }
        }

        /// <summary>
        /// Gets the display name.
        /// </summary>
        public string DisplayName
        {
            get { return _displayName; }
        }
    }
}

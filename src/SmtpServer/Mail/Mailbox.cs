namespace SmtpServer.Mail
{
    /// <summary>
    /// Mailbox
    /// </summary>
    public sealed class Mailbox : IMailbox
    {
        /// <summary>
        /// Empty Mailbox
        /// </summary>
        public static readonly IMailbox Empty = new Mailbox(string.Empty, string.Empty);

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="user">The user/account name.</param>
        /// <param name="host">The host server.</param>
        public Mailbox(string user, string host)
        {
            User = user;
            Host = host;
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="address">The email address to create the mailbox from.</param>
        public Mailbox(string address)
        {
            address = address.Replace(" ", string.Empty);

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
    }
}

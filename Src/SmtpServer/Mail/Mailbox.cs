﻿using System;

namespace SmtpServer.Mail
{
    public sealed class Mailbox : IMailbox
    {
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
        /// Format the mailbox as a string
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return $"{User}@{Host}";
        }
    }
}
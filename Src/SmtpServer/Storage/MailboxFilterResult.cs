namespace SmtpServer.Storage
{
    public enum MailboxFilterResult
    {
        /// <summary>
        /// Accepted.
        /// </summary>
        Yes = 0,

        /// <summary>
        /// Temporarily not accepted.
        /// </summary>
        NoTemporarily = 1,

        /// <summary>
        /// Permanently not accepted.
        /// </summary>
        NoPermanently = 2,

        /// <summary>
        /// The message size limit has been exceeded.
        /// </summary>
        SizeLimitExceeded = 3
    }
}

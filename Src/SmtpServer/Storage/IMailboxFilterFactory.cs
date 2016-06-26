namespace SmtpServer.Storage
{
    public interface IMailboxFilterFactory
    {
        /// <summary>
        /// Creates an instance of the message box filter.
        /// </summary>
        /// <param name="context">The session context.</param>
        /// <returns>The mailbox filter for the session.</returns>
        IMailboxFilter CreateInstance(ISessionContext context);
    }
}
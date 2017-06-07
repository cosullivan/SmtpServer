namespace SmtpServer.Storage
{
    public interface IMessageStoreFactory
    {
        /// <summary>
        /// Creates an instance of the message store for the given session context.
        /// </summary>
        /// <param name="context">The session context.</param>
        /// <returns>The message store instance for the session context.</returns>
        IMessageStore CreateInstance(ISessionContext context);
    }
}
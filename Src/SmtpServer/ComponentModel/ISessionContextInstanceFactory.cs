namespace SmtpServer.ComponentModel
{
    public interface ISessionContextInstanceFactory<out TInstance>
    {
        /// <summary>
        /// Creates an instance of the service for the given session context.
        /// </summary>
        /// <param name="context">The session context.</param>
        /// <returns>The service instance for the session context.</returns>
        TInstance CreateInstance(ISessionContext context);
    }
}
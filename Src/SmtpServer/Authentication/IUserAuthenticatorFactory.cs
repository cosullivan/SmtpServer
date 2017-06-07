namespace SmtpServer.Authentication
{
    public interface IUserAuthenticatorFactory
    {
        /// <summary>
        /// Creates an instance of the user authenticator for the given session context.
        /// </summary>
        /// <param name="context">The session context.</param>
        /// <returns>The user authenticator instance for the session context.</returns>
        IUserAuthenticator CreateInstance(ISessionContext context);
    }
}
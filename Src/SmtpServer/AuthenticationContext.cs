namespace SmtpServer
{
    /// <summary>
    /// Authentication Context
    /// </summary>
    public sealed class AuthenticationContext
    {
        /// <summary>
        /// Authentication Context in the Unauthenticated state
        /// </summary>
        public static readonly AuthenticationContext Unauthenticated = new AuthenticationContext();

        /// <summary>
        /// Constructor.
        /// </summary>
        public AuthenticationContext()
        {
            IsAuthenticated = false;
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="user">The name of the user that was authenticated.</param>
        public AuthenticationContext(string user)
        {
            User = user;
            IsAuthenticated = true;
        }

        /// <summary>
        /// The name of the user that was authenticated.
        /// </summary>
        public string User { get; }

        /// <summary>
        /// Returns a value indicating whether or nor the current session is authenticated.
        /// </summary>
        public bool IsAuthenticated { get; }
    }
}

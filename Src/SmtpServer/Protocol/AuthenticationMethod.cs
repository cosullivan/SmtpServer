namespace SmtpServer.Protocol
{
    /// <summary>
    /// Authentication Method
    /// </summary>
    public enum AuthenticationMethod
    {
        /// <summary>
        /// Login
        /// </summary>
        Login,

        /// <summary>
        /// Plain
        /// </summary>
        Plain,

        /// <summary>
        /// XOAuth2
        /// </summary>
        XOAuth2,

        /// <summary>
        /// OAuthBearer
        /// </summary>
        OAuthBearer
    }
}

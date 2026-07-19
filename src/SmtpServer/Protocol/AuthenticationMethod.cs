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
        /// XOAUTH2 — the bearer-token SASL mechanism used by common mail clients (Google/Microsoft style).
        /// </summary>
        XOAuth2,

        /// <summary>
        /// OAUTHBEARER — the RFC 7628 bearer-token SASL mechanism.
        /// </summary>
        OAuthBearer
    }
}

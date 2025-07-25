namespace SmtpServer
{
    /// <summary>
    /// Choose how MaxMessageSize limit should be considered
    /// </summary>
    public enum MaxMessageSizeHandling
    {
        /// <summary>
        /// Use the size limit for the SIZE extension of ESMTP
        /// </summary>
        Ignore = 0,

        /// <summary>
        /// Close the session after too much data has been sent
        /// </summary>
        Strict = 1,
    }
}

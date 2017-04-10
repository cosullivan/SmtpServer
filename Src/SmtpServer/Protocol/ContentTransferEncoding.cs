namespace SmtpServer.Protocol
{
    public enum ContentTransferEncoding
    {
        /// <summary>
        /// Seven bit encoding (ASCII encoding).
        /// </summary>
        SevenBit = 0,

        /// <summary>
        /// Eight bit encoding (UTF8 Encoding). 
        /// </summary>
        /// <remarks>Up to 998 octets per line with CR and LF (codes 13 and 10 respectively) only allowed to appear as part of a CRLF line ending.</remarks>
        EightBit = 1,

        /// <summary>
        /// Any sequence of octets.
        /// </summary>
        Binary,

        /// <summary>
        /// The default transfer encoding.
        /// </summary>
        Default = SevenBit
    }
}
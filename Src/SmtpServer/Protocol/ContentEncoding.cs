namespace SmtpServer.Protocol
{
    public enum ContentEncoding
    {
        /// <summary>
        /// Seven bit encoding (ASCII encoding).
        /// </summary>
        SevenBit = 0,

        /// <summary>
        /// Eight bit encoding (UTF8 Encoding). 
        /// </summary>
        EightBit = 1,

        /// <summary>
        /// The default transfer encoding.
        /// </summary>
        Default = SevenBit
    }
}
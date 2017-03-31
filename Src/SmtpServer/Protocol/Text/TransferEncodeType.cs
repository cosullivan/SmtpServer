namespace SmtpServer.Protocol.Text
{
    public enum TransferEncodeType
    {
        /// <summary>
        /// No transfer encode was specified.
        /// </summary>
        ASCII = 0,

        /// <summary>
        /// 8BitMime transfer type.
        /// </summary>
        EightBitMime
    }
}

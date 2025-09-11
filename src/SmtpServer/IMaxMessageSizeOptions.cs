namespace SmtpServer
{
    /// <summary>
    /// Defines configuration options for enforcing a maximum allowed message size according to the SMTP SIZE extension (RFC 1870).
    /// Includes the size limit in bytes and the handling strategy for oversized messages.
    /// </summary>
    public interface IMaxMessageSizeOptions
    {
        /// <summary>
        /// Gets or sets the maximum allowed message size in bytes.
        /// </summary>
        int Length { get; }

        /// <summary>
        /// Gets the handling type an oversized message.
        /// </summary>
        MaxMessageSizeHandling Handling { get; }
    }
}

namespace SmtpServer
{
    public interface IMaxMessageSizeOptions
    {
        /// <summary>
        /// Gets the maximum size of a message.
        /// </summary>
        int Length { get; }
        /// <summary>
        /// Gets the handling type an oversized message.
        /// </summary>
        MaxMessageSizeHandling Handling { get; }
    }
}
namespace SmtpServer.Content
{
    public interface IMimeMessage : IMessage
    {
        /// <summary>
        /// The MIME message version.
        /// </summary>
        decimal Version { get; }
    }
}
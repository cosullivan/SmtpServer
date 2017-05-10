namespace SmtpServer.Mime
{
    public interface IMimeHeader
    {
        /// <summary>
        /// The name of the header.
        /// </summary>
        string Name { get; }
    }
}
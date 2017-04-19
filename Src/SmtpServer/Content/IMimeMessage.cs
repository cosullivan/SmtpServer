using SmtpServer.Mime;

namespace SmtpServer.Content
{
    public interface IMimeMessage : IMessage
    {
        /// <summary>
        /// The MIME message version.
        /// </summary>
        MimeVersion Version { get; }
    }
}
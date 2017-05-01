using SmtpServer.Mime;

namespace SmtpServer.Mail
{
    public interface IMimeMessage : IMessage
    {
        /// <summary>
        /// The MIME message version.
        /// </summary>
        MimeVersion Version { get; }
    }
}
using SmtpServer.Mime;

namespace SmtpServer.Mail
{
    public interface IMimeMessage : IMessage
    {
        /// <summary>
        /// The MIME message document.
        /// </summary>
        MimeDocument Document { get; }
    }
}
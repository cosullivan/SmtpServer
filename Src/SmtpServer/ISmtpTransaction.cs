using SmtpServer.Mail;

namespace SmtpServer
{
    public interface ISmtpTransaction : IMimeMessage
    {
        /// <summary>
        /// Reset the current transaction.
        /// </summary>
        void Reset();
    }
}

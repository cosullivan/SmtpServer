using SmtpServer.ComponentModel;

namespace SmtpServer.Storage
{
    /// <summary>
    /// Mailbox Filter Factory Interface
    /// </summary>
    public interface IMailboxFilterFactory : ISessionContextInstanceFactory<IMailboxFilter> { }
}

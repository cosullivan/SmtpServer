using SmtpServer.ComponentModel;

namespace SmtpServer.Storage
{
    public interface IMailboxFilterFactory : ISessionContextInstanceFactory<IMailboxFilter> { }
}
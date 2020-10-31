using SmtpServer.ComponentModel;

namespace SmtpServer.Storage
{
    public interface IMessageStoreFactory : ISessionContextInstanceFactory<IMessageStore> { }
}
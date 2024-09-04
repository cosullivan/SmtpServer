using SmtpServer.ComponentModel;

namespace SmtpServer.Storage
{
    /// <summary>
    /// Message Store Factory Interface
    /// </summary>
    public interface IMessageStoreFactory : ISessionContextInstanceFactory<IMessageStore> { }
}

using SmtpServer.ComponentModel;

namespace SmtpServer.Authentication
{
    public interface IUserAuthenticatorFactory : ISessionContextInstanceFactory<IUserAuthenticator> { }
}
using SmtpServer.ComponentModel;

namespace SmtpServer.Authentication
{
    /// <summary>
    /// User Authenticator Factory Interface
    /// </summary>
    public interface IUserAuthenticatorFactory : ISessionContextInstanceFactory<IUserAuthenticator> { }
}

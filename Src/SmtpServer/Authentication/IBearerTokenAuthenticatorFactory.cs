using SmtpServer.ComponentModel;

namespace SmtpServer.Authentication
{
    /// <summary>
    /// Bearer Token Authenticator Factory Interface
    /// </summary>
    public interface IBearerTokenAuthenticatorFactory : ISessionContextInstanceFactory<IBearerTokenAuthenticator> { }
}

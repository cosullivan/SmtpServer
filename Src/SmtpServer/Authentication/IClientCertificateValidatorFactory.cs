using SmtpServer.ComponentModel;

namespace SmtpServer.Authentication
{
    /// <summary>
    /// Represents a factory for creating instances of <see cref="IClientCertificateValidator"/>.
    /// </summary>
    public interface IClientCertificateValidatorFactory : ISessionContextInstanceFactory<IClientCertificateValidator> { }
}
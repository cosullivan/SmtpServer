using SmtpServer.ComponentModel;

namespace SmtpServer.Protocol
{
    /// <summary>
    /// Factory for creating SMTP command policies per session.
    /// </summary>
    public interface ISmtpCommandPolicyFactory : ISessionContextInstanceFactory<ISmtpCommandPolicy>
    {
    }
}

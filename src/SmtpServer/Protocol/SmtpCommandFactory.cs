using System.Collections.Generic;
using System.Net;
using SmtpServer.Mail;

namespace SmtpServer.Protocol
{
    /// <summary>
    /// Smtp Command Factory
    /// </summary>
    public class SmtpCommandFactory : ISmtpCommandFactory
    {
        /// <inheritdoc />
        public virtual SmtpCommand CreateHelo(string domainOrAddress)
        {
            return new HeloCommand(domainOrAddress);
        }

        /// <inheritdoc />
        public virtual SmtpCommand CreateEhlo(string domainOrAddress)
        {
            return new EhloCommand(domainOrAddress);
        }

        /// <inheritdoc />
        public virtual SmtpCommand CreateMail(IMailbox address, IReadOnlyDictionary<string, string> parameters)
        {
            return new MailCommand(address, parameters);
        }

        /// <inheritdoc />
        public virtual SmtpCommand CreateRcpt(IMailbox address)
        {
            return new RcptCommand(address);
        }

        /// <inheritdoc />
        public virtual SmtpCommand CreateData()
        {
            return new DataCommand();
        }

        /// <inheritdoc />
        public virtual SmtpCommand CreateQuit()
        {
            return new QuitCommand();
        }

        /// <inheritdoc />
        public virtual SmtpCommand CreateNoop()
        {
            return new NoopCommand();
        }

        /// <inheritdoc />
        public virtual SmtpCommand CreateRset()
        {
            return new RsetCommand();
        }

        /// <inheritdoc />
        public virtual SmtpCommand CreateStartTls()
        {
            return new StartTlsCommand();
        }

        /// <inheritdoc />
        public SmtpCommand CreateAuth(AuthenticationMethod method, string parameter)
        {
            return new AuthCommand(method, parameter);
        }

        /// <inheritdoc />
        public virtual SmtpCommand CreateProxy(IPEndPoint sourceEndpoint, IPEndPoint destinationEndpoint)
        {
            return new ProxyCommand(sourceEndpoint, destinationEndpoint);
        }
    }
}

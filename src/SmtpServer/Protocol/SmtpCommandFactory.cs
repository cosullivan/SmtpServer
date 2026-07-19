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
            return CreateRcpt(address, new Dictionary<string, string>());
        }

        /// <inheritdoc />
        public virtual SmtpCommand CreateRcpt(IMailbox address, IReadOnlyDictionary<string, string> parameters)
        {
            return new RcptCommand(address, parameters);
        }

        /// <inheritdoc />
        public virtual SmtpCommand CreateHelp(string argument)
        {
            return new HelpCommand(argument);
        }

        /// <inheritdoc />
        public virtual SmtpCommand CreateVrfy(string argument)
        {
            return new VrfyCommand(argument);
        }

        /// <inheritdoc />
        public virtual SmtpCommand CreateExpn(string argument)
        {
            return new ExpnCommand(argument);
        }

        /// <inheritdoc />
        public virtual SmtpCommand CreateBdat(long size, bool isLast)
        {
            return new BdatCommand(size, isLast);
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

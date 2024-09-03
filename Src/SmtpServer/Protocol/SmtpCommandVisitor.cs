using System;

namespace SmtpServer.Protocol
{
    /// <summary>
    /// Smtp Command Visitor
    /// </summary>
    public abstract class SmtpCommandVisitor
    {
        /// <summary>
        /// Visit the command.
        /// </summary>
        /// <param name="command"></param>
        public void Visit(SmtpCommand command)
        {
            if (command is AuthCommand authCommand)
            {
                Visit(authCommand);
                return;
            }

            if (command is DataCommand dataCommand)
            {
                Visit(dataCommand);
                return;
            }

            if (command is HeloCommand heloCommand)
            {
                Visit(heloCommand);
                return;
            }

            if (command is EhloCommand ehloCommand)
            {
                Visit(ehloCommand);
                return;
            }

            if (command is MailCommand mailCommand)
            {
                Visit(mailCommand);
                return;
            }

            if (command is NoopCommand noopCommand)
            {
                Visit(noopCommand);
                return;
            }

            if (command is ProxyCommand proxyCommand)
            {
                Visit(proxyCommand);
                return;
            }

            if (command is QuitCommand quitCommand)
            {
                Visit(quitCommand);
                return;
            }

            if (command is RcptCommand rcptCommand)
            {
                Visit(rcptCommand);
                return;
            }

            if (command is RsetCommand rsetCommand)
            {
                Visit(rsetCommand);
                return;
            }

            if (command is StartTlsCommand tlsCommand)
            {
                Visit(tlsCommand);
                return;
            }

            throw new NotSupportedException(command.ToString());
        }

        /// <summary>
        /// Visit an AUTH command.
        /// </summary>
        /// <param name="command">The command that is being visited.</param>
        protected virtual void Visit(AuthCommand command) { }

        /// <summary>
        /// Visit an DATA command.
        /// </summary>
        /// <param name="command">The command that is being visited.</param>
        protected virtual void Visit(DataCommand command) { }

        /// <summary>
        /// Visit a HELO command.
        /// </summary>
        /// <param name="command">The command that is being visited.</param>
        protected virtual void Visit(HeloCommand command) { }

        /// <summary>
        /// Visit an EHLO command.
        /// </summary>
        /// <param name="command">The command that is being visited.</param>
        protected virtual void Visit(EhloCommand command) { }

        /// <summary>
        /// Visit an MAIL command.
        /// </summary>
        /// <param name="command">The command that is being visited.</param>
        protected virtual void Visit(MailCommand command) { }

        /// <summary>
        /// Visit an NOOP command.
        /// </summary>
        /// <param name="command">The command that is being visited.</param>
        protected virtual void Visit(NoopCommand command) { }

        /// <summary>
        /// Visit an PROXY command.
        /// </summary>
        /// <param name="command">The command that is being visited.</param>
        protected virtual void Visit(ProxyCommand command) { }

        /// <summary>
        /// Visit an QUIT command.
        /// </summary>
        /// <param name="command">The command that is being visited.</param>
        protected virtual void Visit(QuitCommand command) { }

        /// <summary>
        /// Visit an RCPT command.
        /// </summary>
        /// <param name="command">The command that is being visited.</param>
        protected virtual void Visit(RcptCommand command) { }

        /// <summary>
        /// Visit an RSET command.
        /// </summary>
        /// <param name="command">The command that is being visited.</param>
        protected virtual void Visit(RsetCommand command) { }

        /// <summary>
        /// Visit an STARTTLS command.
        /// </summary>
        /// <param name="command">The command that is being visited.</param>
        protected virtual void Visit(StartTlsCommand command) { }
    }
}

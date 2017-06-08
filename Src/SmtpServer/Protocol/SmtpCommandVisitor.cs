using System;

namespace SmtpServer.Protocol
{
    public abstract class SmtpCommandVisitor
    {
        /// <summary>
        /// Visit the command.
        /// </summary>
        /// <param name="command"></param>
        public void Visit(SmtpCommand command)
        {
            if (command is AuthCommand)
            {
                Visit((AuthCommand)command);
                return;
            }

            if (command is DataCommand)
            {
                Visit((DataCommand)command);
                return;
            }

            if (command is HeloCommand)
            {
                Visit((HeloCommand)command);
                return;
            }

            if (command is EhloCommand)
            {
                Visit((EhloCommand)command);
                return;
            }

            if (command is MailCommand)
            {
                Visit((MailCommand)command);
                return;
            }

            if (command is NoopCommand)
            {
                Visit((NoopCommand)command);
                return;
            }

            if (command is QuitCommand)
            {
                Visit((QuitCommand)command);
                return;
            }

            if (command is RcptCommand)
            {
                Visit((RcptCommand)command);
                return;
            }

            if (command is RsetCommand)
            {
                Visit((RsetCommand)command);
                return;
            }

            if (command is StartTlsCommand)
            {
                Visit((StartTlsCommand)command);
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
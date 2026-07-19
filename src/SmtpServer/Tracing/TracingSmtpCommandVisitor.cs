using System;
using System.IO;
using System.Linq;
using SmtpServer.Mail;
using SmtpServer.Protocol;

namespace SmtpServer.Tracing
{
    /// <summary>
    /// Tracing Smtp Command Visitor
    /// </summary>
    public sealed class TracingSmtpCommandVisitor : SmtpCommandVisitor
    {
        readonly TextWriter _output;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="output">The output stream to write the command execution to.</param>
        public TracingSmtpCommandVisitor(TextWriter output)
        {
            if (output == null)
            {
                throw new ArgumentException(nameof(output));
            }

            _output = output;
        }

        /// <summary>
        /// Visit an AUTH command.
        /// </summary>
        /// <param name="command">The command that is being visited.</param>
        protected override void Visit(AuthCommand command)
        {
            _output.WriteLine("AUTH: Method={0}, Parameter={1}", command.Method, SmtpCommandSnapshot.Redacted);
        }

        /// <summary>
        /// Visit an DATA command.
        /// </summary>
        /// <param name="command">The command that is being visited.</param>
        protected override void Visit(DataCommand command)
        {
            _output.WriteLine("DATA");
        }
        
        /// <summary>
        /// Visit a HELO command.
        /// </summary>
        /// <param name="command">The command that is being visited.</param>
        protected override void Visit(HeloCommand command)
        {
            _output.WriteLine("HELO: DomainOrAddress={0}", command.DomainOrAddress);
        }

        /// <summary>
        /// Visit an EHLO command.
        /// </summary>
        /// <param name="command">The command that is being visited.</param>
        protected override void Visit(EhloCommand command)
        {
            _output.WriteLine("EHLO: DomainOrAddress={0}", command.DomainOrAddress);
        }

        /// <summary>
        /// Visit an MAIL command.
        /// </summary>
        /// <param name="command">The command that is being visited.</param>
        protected override void Visit(MailCommand command)
        {
            _output.WriteLine("MAIL: Address={0} Parameters={1}", 
                command.Address.AsAddress(),
                string.Join(",", command.Parameters.Select(kvp => $"{kvp.Key}={kvp.Value}")));
        }

        /// <summary>
        /// Visit a HELP command.
        /// </summary>
        /// <param name="command">The command that is being visited.</param>
        protected override void Visit(HelpCommand command)
        {
            _output.WriteLine("HELP: Argument={0}", command.Argument);
        }

        /// <summary>
        /// Visit a VRFY command.
        /// </summary>
        /// <param name="command">The command that is being visited.</param>
        protected override void Visit(VrfyCommand command)
        {
            _output.WriteLine("VRFY: Argument={0}", command.Argument);
        }

        /// <summary>
        /// Visit an EXPN command.
        /// </summary>
        /// <param name="command">The command that is being visited.</param>
        protected override void Visit(ExpnCommand command)
        {
            _output.WriteLine("EXPN: Argument={0}", command.Argument);
        }

        /// <summary>
        /// Visit a BDAT command.
        /// </summary>
        /// <param name="command">The command that is being visited.</param>
        protected override void Visit(BdatCommand command)
        {
            _output.WriteLine("BDAT: Size={0}, Last={1}", command.Size, command.IsLast);
        }

        /// <summary>
        /// Visit an NOOP command.
        /// </summary>
        /// <param name="command">The command that is being visited.</param>
        protected override void Visit(NoopCommand command)
        {
            _output.WriteLine("NOOP");
        }

        /// <summary>
        /// Visit an PROXY command.
        /// </summary>
        /// <param name="command">The command that is being visited.</param>
        protected override void Visit(ProxyCommand command)
        {
            _output.WriteLine($"PROXY {command.SourceEndpoint} --> {command.DestinationEndpoint}");
        }

        /// <summary>
        /// Visit an QUIT command.
        /// </summary>
        /// <param name="command">The command that is being visited.</param>
        protected override void Visit(QuitCommand command)
        {
            _output.WriteLine("QUIT");
        }

        /// <summary>
        /// Visit an RCPT command.
        /// </summary>
        /// <param name="command">The command that is being visited.</param>
        protected override void Visit(RcptCommand command)
        {
            _output.WriteLine("RCPT: Address={0} Parameters={1}",
                command.Address.AsAddress(),
                string.Join(",", command.Parameters.Select(kvp => $"{kvp.Key}={kvp.Value}")));
        }

        /// <summary>
        /// Visit an RSET command.
        /// </summary>
        /// <param name="command">The command that is being visited.</param>
        protected override void Visit(RsetCommand command)
        {
            _output.WriteLine("RSET");
        }

        /// <summary>
        /// Visit an STARTTLS command.
        /// </summary>
        /// <param name="command">The command that is being visited.</param>
        protected override void Visit(StartTlsCommand command)
        {
            _output.WriteLine("STARTTLS");
        }
    }
}

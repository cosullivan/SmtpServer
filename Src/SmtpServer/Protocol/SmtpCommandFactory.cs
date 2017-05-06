using System;
using System.Collections.Generic;
using System.Diagnostics;
using SmtpServer.Mail;
using SmtpServer.Text;

namespace SmtpServer.Protocol
{
    public sealed class SmtpCommandFactory : SmtpParser2
    {
        readonly TraceSwitch _logger = new TraceSwitch("SmtpCommandFactory", "SMTP Server Command Factory");
        readonly ISmtpServerOptions _options;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="options">The SMTP server options.</param>
        /// <param name="enumerator">The token enumerator to handle the incoming tokens.</param>
        public SmtpCommandFactory(ISmtpServerOptions options, ITokenEnumerator enumerator) : base(enumerator)
        {
            _options = options;
        }

        /// <summary>
        /// Make a QUIT command.
        /// </summary>
        /// <param name="command">The QUIT command that is defined within the token enumerator.</param>
        /// <param name="errorResponse">The error that indicates why the command could not be made.</param>
        /// <returns>Returns true if a command could be made, false if not.</returns>
        public bool TryMakeQuit(out SmtpCommand command, out SmtpResponse errorResponse)
        {
            Debug.Assert(Enumerator.Peek() == new Token(TokenKind.Text, "QUIT"));

            command = null;
            errorResponse = null;

            Enumerator.Take();

            if (TryMakeEnd() == false)
            {
                _logger.LogVerbose("QUIT command can not have parameters.");

                errorResponse = SmtpResponse.SyntaxError;
                return false;
            }

            command = new QuitCommand(_options);
            return true;
        }

        /// <summary>
        /// Make an NOOP command from the given enumerator.
        /// </summary>
        /// <param name="command">The NOOP command that is defined within the token enumerator.</param>
        /// <param name="errorResponse">The error that indicates why the command could not be made.</param>
        /// <returns>Returns true if a command could be made, false if not.</returns>
        public bool TryMakeNoop(out SmtpCommand command, out SmtpResponse errorResponse)
        {
            Debug.Assert(Enumerator.Peek() == new Token(TokenKind.Text, "NOOP"));

            command = null;
            errorResponse = null;

            Enumerator.Take();

            if (TryMakeEnd() == false)
            {
                _logger.LogVerbose("NOOP command can not have parameters.");

                errorResponse = SmtpResponse.SyntaxError;
                return false;
            }

            command = new NoopCommand(_options);
            return true;
        }

        /// <summary>
        /// Make an RSET command from the given enumerator.
        /// </summary>
        /// <param name="command">The RSET command that is defined within the token enumerator.</param>
        /// <param name="errorResponse">The error that indicates why the command could not be made.</param>
        /// <returns>Returns true if a command could be made, false if not.</returns>
        public bool TryMakeRset(out SmtpCommand command, out SmtpResponse errorResponse)
        {
            Debug.Assert(Enumerator.Peek() == new Token(TokenKind.Text, "RSET"));

            command = null;
            errorResponse = null;

            Enumerator.Take();

            if (TryMakeEnd() == false)
            {
                _logger.LogVerbose("RSET command can not have parameters.");

                errorResponse = SmtpResponse.SyntaxError;
                return false;
            }

            command = new RsetCommand(_options);
            return true;
        }

        /// <summary>
        /// Make a HELO command from the given enumerator.
        /// </summary>
        /// <param name="command">The HELO command that is defined within the token enumerator.</param>
        /// <param name="errorResponse">The error that indicates why the command could not be made.</param>
        /// <returns>Returns true if a command could be made, false if not.</returns>
        public bool TryMakeHelo(out SmtpCommand command, out SmtpResponse errorResponse)
        {
            Debug.Assert(Enumerator.Peek() == new Token(TokenKind.Text, "HELO"));

            command = null;
            errorResponse = null;

            Enumerator.Take();
            Enumerator.Skip(TokenKind.Space);

            string domain;
            if (TryMakeDomain(out domain) == false)
            {
                _logger.LogVerbose("Could not match the domain name (Text={0}).", CompleteTokenizedText());

                errorResponse = SmtpResponse.SyntaxError;
                return false;
            }

            command = new HeloCommand(_options, domain);
            return true;
        }

        /// <summary>
        /// Make an EHLO command from the given enumerator.
        /// </summary>
        /// <param name="command">The EHLO command that is defined within the token enumerator.</param>
        /// <param name="errorResponse">The error that indicates why the command could not be made.</param>
        /// <returns>Returns true if a command could be made, false if not.</returns>
        public bool TryMakeEhlo(out SmtpCommand command, out SmtpResponse errorResponse)
        {
            Debug.Assert(Enumerator.Peek() == new Token(TokenKind.Text, "EHLO"));

            command = null;
            errorResponse = null;

            Enumerator.Take();
            Enumerator.Skip(TokenKind.Space);

            string domain;
            if (TryMakeDomain(out domain))
            {
                command = new EhloCommand(_options, domain);
                return true;
            }

            string address;
            if (TryMakeAddressLiteral(out address))
            {
                command = new EhloCommand(_options, address);
                return true;
            }

            errorResponse = SmtpResponse.SyntaxError;
            return false;
        }

        /// <summary>
        /// Make a MAIL command from the given enumerator.
        /// </summary>
        /// <param name="command">The MAIL command that is defined within the token enumerator.</param>
        /// <param name="errorResponse">The error that indicates why the command could not be made.</param>
        /// <returns>Returns true if a command could be made, false if not.</returns>
        public bool TryMakeMail(out SmtpCommand command, out SmtpResponse errorResponse)
        {
            Debug.Assert(Enumerator.Peek() == new Token(TokenKind.Text, "MAIL"));

            command = null;
            errorResponse = null;

            Enumerator.Take();
            Enumerator.Skip(TokenKind.Space);

            if (Enumerator.Take() != new Token(TokenKind.Text, "FROM") || Enumerator.Take() != new Token(TokenKind.Other, ":"))
            {
                errorResponse = new SmtpResponse(SmtpReplyCode.SyntaxError, "missing the FROM:");
                return false;
            }

            // according to the spec, whitespace isnt allowed here but most servers send it
            Enumerator.Skip(TokenKind.Space);

            IMailbox mailbox;
            if (TryMakeReversePath(out mailbox) == false)
            {
                _logger.LogVerbose("Syntax Error (Text={0})", CompleteTokenizedText());

                errorResponse = new SmtpResponse(SmtpReplyCode.SyntaxError);
                return false;
            }

            Enumerator.Skip(TokenKind.Space);

            // match the optional (ESMTP) parameters
            IReadOnlyDictionary<string, string> parameters;
            if (TryMakeMailParameters(out parameters) == false)
            {
                parameters = new Dictionary<string, string>();
            }

            command = new MailCommand(_options, mailbox, parameters);
            return true;
        }

        /// <summary>
        /// Make a RCTP command from the given enumerator.
        /// </summary>
        /// <param name="command">The RCTP command that is defined within the token enumerator.</param>
        /// <param name="errorResponse">The error that indicates why the command could not be made.</param>
        /// <returns>Returns true if a command could be made, false if not.</returns>
        public bool TryMakeRcpt(out SmtpCommand command, out SmtpResponse errorResponse)
        {
            Debug.Assert(Enumerator.Peek() == new Token(TokenKind.Text, "RCPT"));

            command = null;
            errorResponse = null;

            Enumerator.Take();
            Enumerator.Skip(TokenKind.Space);

            if (Enumerator.Take() != new Token(TokenKind.Text, "TO") || Enumerator.Take() != new Token(TokenKind.Other, ":"))
            {
                errorResponse = new SmtpResponse(SmtpReplyCode.SyntaxError, "missing the TO:");
                return false;
            }

            // according to the spec, whitespace isnt allowed here anyway
            Enumerator.Skip(TokenKind.Space);

            IMailbox mailbox;
            if (TryMakePath(out mailbox) == false)
            {
                _logger.LogVerbose("Syntax Error (Text={0})", CompleteTokenizedText());

                errorResponse = SmtpResponse.SyntaxError;
                return false;
            }

            // TODO: support optional service extension parameters here

            command = new RcptCommand(_options, mailbox);
            return true;
        }

        /// <summary>
        /// Make a DATA command from the given enumerator.
        /// </summary>
        /// <param name="command">The DATA command that is defined within the token enumerator.</param>
        /// <param name="errorResponse">The error that indicates why the command could not be made.</param>
        /// <returns>Returns true if a command could be made, false if not.</returns>
        public bool TryMakeData(out SmtpCommand command, out SmtpResponse errorResponse)
        {
            Debug.Assert(Enumerator.Peek() == new Token(TokenKind.Text, "DATA"));

            command = null;
            errorResponse = null;

            Enumerator.Take();
            Enumerator.Skip(TokenKind.Space);

            if (TryMakeEnd() == false)
            {
                _logger.LogVerbose("DATA command can not have parameters.");

                errorResponse = SmtpResponse.SyntaxError;
                return false;
            }

            command = new DataCommand(_options);
            return true;
        }

        /// <summary>
        /// Make an DBUG command from the given enumerator.
        /// </summary>
        /// <param name="command">The DBUG command that is defined within the token enumerator.</param>
        /// <param name="errorResponse">The error that indicates why the command could not be made.</param>
        /// <returns>Returns true if a command could be made, false if not.</returns>
        public bool TryMakeDbug(out SmtpCommand command, out SmtpResponse errorResponse)
        {
            Debug.Assert(Enumerator.Peek() == new Token(TokenKind.Text, "DBUG"));

            command = null;
            errorResponse = null;

            Enumerator.Take();
            Enumerator.Skip(TokenKind.Space);

            if (TryMakeEnd() == false)
            {
                _logger.LogVerbose("DBUG command can not have parameters.");

                errorResponse = SmtpResponse.SyntaxError;
                return false;
            }

            command = new DbugCommand(_options);
            return true;
        }

        /// <summary>
        /// Make an STARTTLS command from the given enumerator.
        /// </summary>
        /// <param name="command">The STARTTLS command that is defined within the token enumerator.</param>
        /// <param name="errorResponse">The error that indicates why the command could not be made.</param>
        /// <returns>Returns true if a command could be made, false if not.</returns>
        public bool TryMakeStartTls(out SmtpCommand command, out SmtpResponse errorResponse)
        {
            Debug.Assert(Enumerator.Peek() == new Token(TokenKind.Text, "STARTTLS"));

            command = null;
            errorResponse = null;

            Enumerator.Take();
            Enumerator.Skip(TokenKind.Space);

            if (TryMakeEnd() == false)
            {
                _logger.LogVerbose("STARTTLS command can not have parameters.");

                errorResponse = SmtpResponse.SyntaxError;
                return false;
            }

            command = new StartTlsCommand(_options);
            return true;
        }

        /// <summary>
        /// Make an AUTH command from the given enumerator.
        /// </summary>
        /// <param name="command">The AUTH command that is defined within the token enumerator.</param>
        /// <param name="errorResponse">The error that indicates why the command could not be made.</param>
        /// <returns>Returns true if a command could be made, false if not.</returns>
        public bool TryMakeAuth(out SmtpCommand command, out SmtpResponse errorResponse)
        {
            Debug.Assert(Enumerator.Peek() == new Token(TokenKind.Text, "AUTH"));

            command = null;
            errorResponse = null;

            Enumerator.Take();
            Enumerator.Skip(TokenKind.Space);

            AuthenticationMethod method;
            if (Enum.TryParse(Enumerator.Peek().Text, true, out method) == false)
            {
                _logger.LogVerbose("AUTH command requires a valid method (PLAIN or LOGIN)");

                errorResponse = SmtpResponse.SyntaxError;
                return false;
            }

            Enumerator.Take();

            string parameter = null;
            if (TryMake(TryMakeEnd) == false && TryMakeBase64(out parameter) == false)
            {
                _logger.LogVerbose("AUTH parameter must be a Base64 encoded string");

                errorResponse = SmtpResponse.SyntaxError;
                return false;
            }

            command = new AuthCommand(_options, method, parameter);
            return true;
        }

        /// <summary>
        /// Attempt to make the end of the line.
        /// </summary>
        /// <returns>true if the end of the line could be made, false if not.</returns>
        bool TryMakeEnd()
        {
            Enumerator.Skip(TokenKind.Space);

            return Enumerator.Take() == Token.None;
        }

        /// <summary>
        /// Returns the complete tokenized text.
        /// </summary>
        /// <returns>The complete tokenized text.</returns>
        string CompleteTokenizedText()
        {
            throw new NotImplementedException();
        }
    }
}
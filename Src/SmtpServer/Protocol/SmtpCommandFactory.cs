using System;
using System.Collections.Generic;
using System.Diagnostics;
using SmtpServer.Mail;
using SmtpServer.Protocol.Text;

namespace SmtpServer.Protocol
{
    public sealed class SmtpCommandFactory
    {
        readonly TraceSwitch _logger = new TraceSwitch("SmtpCommandFactory", "SMTP Server Command Factory");
        readonly ISmtpServerOptions _options;
        readonly SmtpParser _parser;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="options">The SMTP server options.</param>
        /// <param name="parser">The ABNF SMTP argument parser.</param>
        public SmtpCommandFactory(ISmtpServerOptions options, SmtpParser parser)
        {
            _options = options;
            _parser = parser;
        }

        /// <summary>
        /// Make a QUIT command.
        /// </summary>
        /// <param name="enumerator">The enumerator to create the command from.</param>
        /// <param name="command">The QUIT command that is defined within the token enumerator.</param>
        /// <param name="errorResponse">The error that indicates why the command could not be made.</param>
        /// <returns>Returns true if a command could be made, false if not.</returns>
        public bool TryMakeQuit(TokenEnumerator enumerator, out SmtpCommand command, out SmtpResponse errorResponse)
        {
            Debug.Assert(enumerator.Peek() == new Token(TokenKind.Text, "QUIT"));

            command = null;
            errorResponse = null;

            enumerator.Take();
            enumerator.TakeWhile(TokenKind.Space);

            if (enumerator.Count > 1)
            {
                _logger.LogVerbose("QUIT command can not have parameters (found {0} parameters).", enumerator.Count);

                errorResponse = SmtpResponse.SyntaxError;
                return false;
            }

            command = QuitCommand.Instance;
            return true;
        }

        /// <summary>
        /// Make an NOOP command from the given enumerator.
        /// </summary>
        /// <param name="enumerator">The enumerator to create the command from.</param>
        /// <param name="command">The NOOP command that is defined within the token enumerator.</param>
        /// <param name="errorResponse">The error that indicates why the command could not be made.</param>
        /// <returns>Returns true if a command could be made, false if not.</returns>
        public bool TryMakeNoop(TokenEnumerator enumerator, out SmtpCommand command, out SmtpResponse errorResponse)
        {
            Debug.Assert(enumerator.Peek() == new Token(TokenKind.Text, "NOOP"));

            command = null;
            errorResponse = null;

            enumerator.Take();
            enumerator.TakeWhile(TokenKind.Space);

            if (enumerator.Count > 1)
            {
                _logger.LogVerbose("NOOP command can not have parameters (found {0} parameters).", enumerator.Count);

                errorResponse = SmtpResponse.SyntaxError;
                return false;
            }

            command = NoopCommand.Instance;
            return true;
        }

        /// <summary>
        /// Make an RSET command from the given enumerator.
        /// </summary>
        /// <param name="enumerator">The enumerator to create the command from.</param>
        /// <param name="command">The RSET command that is defined within the token enumerator.</param>
        /// <param name="errorResponse">The error that indicates why the command could not be made.</param>
        /// <returns>Returns true if a command could be made, false if not.</returns>
        public bool TryMakeRset(TokenEnumerator enumerator, out SmtpCommand command, out SmtpResponse errorResponse)
        {
            Debug.Assert(enumerator.Peek() == new Token(TokenKind.Text, "RSET"));

            command = null;
            errorResponse = null;

            enumerator.Take();
            enumerator.TakeWhile(TokenKind.Space);

            if (enumerator.Count > 1)
            {
                _logger.LogVerbose("RSET command can not have parameters (found {0} parameters).", enumerator.Count);

                errorResponse = SmtpResponse.SyntaxError;
                return false;
            }

            command = RsetCommand.Instance;
            return true;
        }

        /// <summary>
        /// Make a HELO command from the given enumerator.
        /// </summary>
        /// <param name="enumerator">The enumerator to create the command from.</param>
        /// <param name="command">The HELO command that is defined within the token enumerator.</param>
        /// <param name="errorResponse">The error that indicates why the command could not be made.</param>
        /// <returns>Returns true if a command could be made, false if not.</returns>
        public bool TryMakeHelo(TokenEnumerator enumerator, out SmtpCommand command, out SmtpResponse errorResponse)
        {
            Debug.Assert(enumerator.Peek() == new Token(TokenKind.Text, "HELO"));

            command = null;
            errorResponse = null;

            enumerator.Take();
            enumerator.TakeWhile(TokenKind.Space);

            string domain;
            if (_parser.TryMakeDomain(enumerator, out domain) == false)
            {
                _logger.LogVerbose("Could not match the domain name (Text={0}).", enumerator.AsText());

                errorResponse = SmtpResponse.SyntaxError;
                return false;
            }

            command = new HeloCommand(domain);
            return true;
        }

        /// <summary>
        /// Make an EHLO command from the given enumerator.
        /// </summary>
        /// <param name="enumerator">The enumerator to create the command from.</param>
        /// <param name="command">The EHLO command that is defined within the token enumerator.</param>
        /// <param name="errorResponse">The error that indicates why the command could not be made.</param>
        /// <returns>Returns true if a command could be made, false if not.</returns>
        public bool TryMakeEhlo(TokenEnumerator enumerator, out SmtpCommand command, out SmtpResponse errorResponse)
        {
            Debug.Assert(enumerator.Peek() == new Token(TokenKind.Text, "EHLO"));

            command = null;
            errorResponse = null;

            enumerator.Take();
            enumerator.TakeWhile(TokenKind.Space);

            string domain;
            if (_parser.TryMakeDomain(enumerator, out domain))
            {
                command = new EhloCommand(domain, _options);
                return true;
            }

            string address;
            if (_parser.TryMakeAddressLiteral(enumerator, out address))
            {
                command = new EhloCommand(address, _options);
                return true;
            }

            errorResponse = SmtpResponse.SyntaxError;
            return false;
        }

        /// <summary>
        /// Make a MAIL command from the given enumerator.
        /// </summary>
        /// <param name="enumerator">The enumerator to create the command from.</param>
        /// <param name="command">The MAIL command that is defined within the token enumerator.</param>
        /// <param name="errorResponse">The error that indicates why the command could not be made.</param>
        /// <returns>Returns true if a command could be made, false if not.</returns>
        public bool TryMakeMail(TokenEnumerator enumerator, out SmtpCommand command, out SmtpResponse errorResponse)
        {
            Debug.Assert(enumerator.Peek() == new Token(TokenKind.Text, "MAIL"));

            command = null;
            errorResponse = null;

            enumerator.Take();
            enumerator.TakeWhile(TokenKind.Space);

            if (enumerator.Take() != new Token(TokenKind.Text, "FROM") || enumerator.Take() != new Token(TokenKind.Punctuation, ":"))
            {
                errorResponse = new SmtpResponse(SmtpReplyCode.SyntaxError, "missing the FROM:");
                return false;
            }

            // according to the spec, whitespace isnt allowed here but most servers send it
            enumerator.TakeWhile(TokenKind.Space);

            IMailbox mailbox;
            if (_parser.TryMakeReversePath(enumerator, out mailbox) == false)
            {
                _logger.LogVerbose("Syntax Error (Text={0})", enumerator.AsText());

                errorResponse = new SmtpResponse(SmtpReplyCode.SyntaxError);
                return false;
            }

            // match the optional (ESMTP) parameters
            IDictionary<string, string> parameters;
            if (_parser.TryMakeMailParameters(enumerator, out parameters) == false)
            {
                parameters = new Dictionary<string, string>();
            }

            command = new MailCommand(mailbox, parameters, _options.MailboxFilter);
            return true;
        }

        /// <summary>
        /// Make a RCTP command from the given enumerator.
        /// </summary>
        /// <param name="enumerator">The enumerator to create the command from.</param>
        /// <param name="command">The RCTP command that is defined within the token enumerator.</param>
        /// <param name="errorResponse">The error that indicates why the command could not be made.</param>
        /// <returns>Returns true if a command could be made, false if not.</returns>
        public bool TryMakeRcpt(TokenEnumerator enumerator, out SmtpCommand command, out SmtpResponse errorResponse)
        {
            Debug.Assert(enumerator.Peek() == new Token(TokenKind.Text, "RCPT"));

            command = null;
            errorResponse = null;

            enumerator.Take();
            enumerator.TakeWhile(TokenKind.Space);

            if (enumerator.Take() != new Token(TokenKind.Text, "TO") || enumerator.Take() != new Token(TokenKind.Punctuation, ":"))
            {
                errorResponse = new SmtpResponse(SmtpReplyCode.SyntaxError, "missing the TO:");
                return false;
            }

            // according to the spec, whitespace isnt allowed here anyway
            enumerator.TakeWhile(TokenKind.Space);

            IMailbox mailbox;
            if (_parser.TryMakePath(enumerator, out mailbox) == false)
            {
                _logger.LogVerbose("Syntax Error (Text={0})", enumerator.AsText());

                errorResponse = SmtpResponse.SyntaxError;
                return false;
            }

            // TODO: support optional service extension parameters here

            command = new RcptCommand(mailbox, _options.MailboxFilter);
            return true;
        }

        /// <summary>
        /// Make a DATA command from the given enumerator.
        /// </summary>
        /// <param name="enumerator">The enumerator to create the command from.</param>
        /// <param name="command">The DATA command that is defined within the token enumerator.</param>
        /// <param name="errorResponse">The error that indicates why the command could not be made.</param>
        /// <returns>Returns true if a command could be made, false if not.</returns>
        public bool TryMakeData(TokenEnumerator enumerator, out SmtpCommand command, out SmtpResponse errorResponse)
        {
            Debug.Assert(enumerator.Peek() == new Token(TokenKind.Text, "DATA"));

            command = null;
            errorResponse = null;

            enumerator.Take();
            enumerator.TakeWhile(TokenKind.Space);

            if (enumerator.Count > 1)
            {
                _logger.LogVerbose("DATA command can not have parameters (Tokens={0})", enumerator.Count);

                errorResponse = SmtpResponse.SyntaxError;
                return false;
            }

            command = new DataCommand(_options.MessageStore);
            return true;
        }

        /// <summary>
        /// Make an DBUG command from the given enumerator.
        /// </summary>
        /// <param name="enumerator">The enumerator to create the command from.</param>
        /// <param name="command">The DBUG command that is defined within the token enumerator.</param>
        /// <param name="errorResponse">The error that indicates why the command could not be made.</param>
        /// <returns>Returns true if a command could be made, false if not.</returns>
        public bool TryMakeDbug(TokenEnumerator enumerator, out SmtpCommand command, out SmtpResponse errorResponse)
        {
            Debug.Assert(enumerator.Peek() == new Token(TokenKind.Text, "DBUG"));

            command = null;
            errorResponse = null;

            enumerator.Take();
            enumerator.TakeWhile(TokenKind.Space);

            if (enumerator.Count > 1)
            {
                _logger.LogVerbose("DBUG command can not have parameters (Tokens={0})", enumerator.Count);

                errorResponse = SmtpResponse.SyntaxError;
                return false;
            }

            command = DbugCommand.Instance;
            return true;
        }

        /// <summary>
        /// Make an STARTTLS command from the given enumerator.
        /// </summary>
        /// <param name="enumerator">The enumerator to create the command from.</param>
        /// <param name="command">The STARTTLS command that is defined within the token enumerator.</param>
        /// <param name="errorResponse">The error that indicates why the command could not be made.</param>
        /// <returns>Returns true if a command could be made, false if not.</returns>
        public bool TryMakeStartTls(TokenEnumerator enumerator, out SmtpCommand command, out SmtpResponse errorResponse)
        {
            Debug.Assert(enumerator.Peek() == new Token(TokenKind.Text, "STARTTLS"));

            command = null;
            errorResponse = null;

            enumerator.Take();
            enumerator.TakeWhile(TokenKind.Space);

            if (enumerator.Count > 1)
            {
                _logger.LogVerbose("STARTTLS command can not have parameters (Tokens={0})", enumerator.Count);

                errorResponse = SmtpResponse.SyntaxError;
                return false;
            }

            command = new StartTlsCommand(_options.ServerCertificate);
            return true;
        }

        /// <summary>
        /// Make an AUTH command from the given enumerator.
        /// </summary>
        /// <param name="enumerator">The enumerator to create the command from.</param>
        /// <param name="command">The AUTH command that is defined within the token enumerator.</param>
        /// <param name="errorResponse">The error that indicates why the command could not be made.</param>
        /// <returns>Returns true if a command could be made, false if not.</returns>
        public bool TryMakeAuth(TokenEnumerator enumerator, out SmtpCommand command, out SmtpResponse errorResponse)
        {
            Debug.Assert(enumerator.Peek() == new Token(TokenKind.Text, "AUTH"));

            command = null;
            errorResponse = null;

            enumerator.Take();
            enumerator.TakeWhile(TokenKind.Space);

            AuthenticationMethod method;
            if (Enum.TryParse(enumerator.Peek().Text, true, out method) == false)
            {
                _logger.LogVerbose("AUTH command requires a valid method (PLAIN or LOGIN)");

                errorResponse = SmtpResponse.SyntaxError;
                return false;
            }

            enumerator.Take();

            string parameter = null;
            if (enumerator.Count > 0 && _parser.TryMakeBase64(enumerator, out parameter) == false)
            {
                _logger.LogVerbose("AUTH parameter must be a Base64 encoded string");

                errorResponse = SmtpResponse.SyntaxError;
                return false;
            }

            command = new AuthCommand(_options.UserAuthenticator, method, parameter);
            return true;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using SmtpServer.Mail;
using SmtpServer.Text;

namespace SmtpServer.Protocol
{
    /// <remarks>
    /// This class is responsible for parsing the SMTP command arguments according to the ANBF described in
    /// the RFC http://tools.ietf.org/html/rfc5321#section-4.1.2
    /// </remarks>
    public class SmtpParser : TokenParser
    {
        #region Tokens

        static class Tokens
        {
            // ReSharper disable InconsistentNaming
            internal static readonly Token Hyphen = Token.Create('-');
            internal static readonly Token Colon = Token.Create(':');
            internal static readonly Token LessThan = Token.Create('<');
            internal static readonly Token GreaterThan = Token.Create('>');
            internal static readonly Token Comma = Token.Create(',');
            internal static readonly Token At = Token.Create('@');
            internal static readonly Token Period = Token.Create('.');
            internal static readonly Token LeftBracket = Token.Create('[');
            internal static readonly Token RightBracket = Token.Create(']');
            internal static readonly Token Quote = Token.Create('"');
            internal static readonly Token Equal = Token.Create('=');
            internal static readonly Token BackSlash = Token.Create('\\');

            internal static class Text
            {
                internal static readonly Token From = Token.Create("FROM");
                internal static readonly Token To = Token.Create("TO");
                internal static readonly Token IpVersionTag = Token.Create("IPv");
                internal static readonly Token Unknown = Token.Create("UNKNOWN");
                internal static readonly Token Tcp = Token.Create("TCP");
            }

            internal static class Numbers
            {
                internal static readonly Token Four = Token.Create(TokenKind.Number, "4");
                internal static readonly Token Six = Token.Create(TokenKind.Number, "6");
            }
            // ReSharper restore InconsistentNaming
        }

        #endregion

        readonly ISmtpServerOptions _options;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="options">The SMTP server options.</param>
        /// <param name="enumerator">The token enumerator to handle the incoming tokens.</param>
        public SmtpParser(ISmtpServerOptions options, ITokenEnumerator enumerator) : base(enumerator)
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
            command = null;
            errorResponse = null;

            Enumerator.Take();

            if (TryMakeEnd() == false)
            {
                _options.Logger.LogVerbose("QUIT command can not have parameters.");

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
            command = null;
            errorResponse = null;

            Enumerator.Take();

            if (TryMakeEnd() == false)
            {
                _options.Logger.LogVerbose("NOOP command can not have parameters.");

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
            command = null;
            errorResponse = null;

            Enumerator.Take();

            if (TryMakeEnd() == false)
            {
                _options.Logger.LogVerbose("RSET command can not have parameters.");

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
            command = null;
            errorResponse = null;

            Enumerator.Take();
            Enumerator.Skip(TokenKind.Space);

            if (TryMakeDomain(out var domain))
            {
                command = new HeloCommand(_options, domain);
                return true;
            }

            // according to RFC5321 the HELO command should only accept the Domain
            // and not the address literal, however some mail clients will send the
            // address literal and there is no harm in accepting it
            if (TryMakeAddressLiteral(out var address))
            {
                command = new HeloCommand(_options, address);
                return true;
            }

            errorResponse = SmtpResponse.SyntaxError;
            return false;
        }

        /// <summary>
        /// Make an EHLO command from the given enumerator.
        /// </summary>
        /// <param name="command">The EHLO command that is defined within the token enumerator.</param>
        /// <param name="errorResponse">The error that indicates why the command could not be made.</param>
        /// <returns>Returns true if a command could be made, false if not.</returns>
        public bool TryMakeEhlo(out SmtpCommand command, out SmtpResponse errorResponse)
        {
            command = null;
            errorResponse = null;

            Enumerator.Take();
            Enumerator.Skip(TokenKind.Space);

            if (TryMakeDomain(out var domain))
            {
                command = new EhloCommand(_options, domain);
                return true;
            }

            if (TryMakeAddressLiteral(out var address))
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
            command = null;
            errorResponse = null;

            Enumerator.Take();
            Enumerator.Skip(TokenKind.Space);

            if (Enumerator.Take() != Tokens.Text.From || Enumerator.Take() != Tokens.Colon)
            {
                errorResponse = new SmtpResponse(SmtpReplyCode.SyntaxError, "missing the FROM:");
                return false;
            }

            // according to the spec, whitespace isnt allowed here but most servers send it
            Enumerator.Skip(TokenKind.Space);

            if (TryMakeReversePath(out var mailbox) == false)
            {
                _options.Logger.LogVerbose("Syntax Error (Text={0})", CompleteTokenizedText());

                errorResponse = new SmtpResponse(SmtpReplyCode.SyntaxError);
                return false;
            }

            Enumerator.Skip(TokenKind.Space);

            // match the optional (ESMTP) parameters
            if (TryMakeMailParameters(out var parameters) == false)
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
            command = null;
            errorResponse = null;

            Enumerator.Take();
            Enumerator.Skip(TokenKind.Space);

            if (Enumerator.Take() != Tokens.Text.To || Enumerator.Take() != Tokens.Colon)
            {
                errorResponse = new SmtpResponse(SmtpReplyCode.SyntaxError, "missing the TO:");
                return false;
            }

            // according to the spec, whitespace isnt allowed here anyway
            Enumerator.Skip(TokenKind.Space);

            if (TryMakePath(out var mailbox) == false)
            {
                _options.Logger.LogVerbose("Syntax Error (Text={0})", CompleteTokenizedText());

                errorResponse = SmtpResponse.SyntaxError;
                return false;
            }

            // TODO: support optional service extension parameters here

            command = new RcptCommand(_options, mailbox);
            return true;
        }

        /// <summary>
        /// Support proxy protocol version 1 header for use with HAProxy.
        /// Documented at http://www.haproxy.org/download/1.8/doc/proxy-protocol.txt
        /// </summary>
        /// <param name="command">The PROXY command that is defined within the token enumerator.</param>
        /// <param name="errorResponse">The error that indicates why the command could not be made.</param>
        /// <returns>Returns true if a command could be made, false if not.</returns>
        public bool TryMakeProxy(out SmtpCommand command, out SmtpResponse errorResponse)
        {
            // ABNF
            // proxy            = "PROXY" space ( unknown-proxy | tcp4-proxy | tcp6-proxy )
            // unknown-proxy    = "UNKNOWN"
            // tcp4-proxy       = "TCP4" space ipv4-address-literal space ipv4-address-literal space ip-port-number space ip-port-number   
            // tcp6-proxy       = "TCP6" space ipv6-address-literal space ipv6-address-literal space ip-port-number space ip-port-number
            // space            = " "
            // ip-port          = wnum
            // wnum             = 1*5DIGIT ; in the range of 0-65535

            command = null;
            errorResponse = null;

            Enumerator.Take();
            Enumerator.Skip(TokenKind.Space);

            if (TryMake(TryMakeUnknownProxy, out command))
            {
                return true;
            }

            if (TryMake(TryMakeTcp4Proxy, out command))
            {
                return true;
            }

            return TryMakeTcp6Proxy(out command);
        }

        /// <summary>
        /// Attempt to make the Unknown Proxy command.
        /// </summary>
        /// <param name="command">The command that was made.</param>
        /// <returns>true if the command was made, false if not.</returns>
        bool TryMakeUnknownProxy(out SmtpCommand command)
        {
            command = new ProxyCommand(_options);

            return Enumerator.Take() == Tokens.Text.Unknown;
        }

        bool TryMakeTcp4Proxy(out SmtpCommand command)
        {
            command = null;

            if (Enumerator.Take() != Tokens.Text.Tcp)
            {
                return false;
            }

            if (Enumerator.Take() != Tokens.Numbers.Four)
            {
                return false;
            }

            return TryMakeProxyAddresses(TryMakeIPv4AddressLiteral, out command);
        }

        bool TryMakeTcp6Proxy(out SmtpCommand command)
        {
            command = null;

            if (Enumerator.Take() != Tokens.Text.Tcp)
            {
                return false;
            }

            if (Enumerator.Take() != Tokens.Numbers.Six)
            {
                return false;
            }

            return TryMakeProxyAddresses(TryMakeIPv6Address, out command);
        }

        bool TryMakeProxyAddresses(TryMakeDelegate<string> tryMakeAddressDelegate, out SmtpCommand command)
        {
            command = null;
            
            Enumerator.Skip(TokenKind.Space);

            if (tryMakeAddressDelegate(out var sourceAddress) == false)
            {
                return false;
            }

            Enumerator.Skip(TokenKind.Space);

            if (tryMakeAddressDelegate(out var destinationAddress) == false)
            {
                return false;
            }

            Enumerator.Skip(TokenKind.Space);

            if (TryMakeWnum(out var sourcePort) == false)
            {
                return false;
            }

            Enumerator.Skip(TokenKind.Space);

            if (TryMakeWnum(out var destinationPort) == false)
            {
                return false;
            }

            command = new ProxyCommand(_options, new IPEndPoint(IPAddress.Parse(sourceAddress), sourcePort), new IPEndPoint(IPAddress.Parse(destinationAddress), destinationPort));
            return true;
        }

        bool TryMakeWnum(out int wnum)
        {
            wnum = default;

            var token = Enumerator.Take();

            if (token.Kind == TokenKind.Number && int.TryParse(token.Text, out wnum))
            {
                return wnum >= 0 && wnum <= 65535;
            }

            return false;
        }

        /// <summary>
        /// Make a DATA command from the given enumerator.
        /// </summary>
        /// <param name="command">The DATA command that is defined within the token enumerator.</param>
        /// <param name="errorResponse">The error that indicates why the command could not be made.</param>
        /// <returns>Returns true if a command could be made, false if not.</returns>
        public bool TryMakeData(out SmtpCommand command, out SmtpResponse errorResponse)
        {
            command = null;
            errorResponse = null;

            Enumerator.Take();
            Enumerator.Skip(TokenKind.Space);

            if (TryMakeEnd() == false)
            {
                _options.Logger.LogVerbose("DATA command can not have parameters.");

                errorResponse = SmtpResponse.SyntaxError;
                return false;
            }

            command = new DataCommand(_options);
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
            command = null;
            errorResponse = null;

            Enumerator.Take();
            Enumerator.Skip(TokenKind.Space);

            if (TryMakeEnd() == false)
            {
                _options.Logger.LogVerbose("STARTTLS command can not have parameters.");

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
            command = null;
            errorResponse = null;

            Enumerator.Take();
            Enumerator.Skip(TokenKind.Space);

            if (Enum.TryParse(Enumerator.Take().Text, true, out AuthenticationMethod method) == false)
            {
                _options.Logger.LogVerbose("AUTH command requires a valid method (PLAIN or LOGIN)");

                errorResponse = SmtpResponse.SyntaxError;
                return false;
            }

            Enumerator.Take();

            string parameter = null;
            if (TryMake(TryMakeEnd) == false && TryMakeBase64(out parameter) == false)
            {
                _options.Logger.LogVerbose("AUTH parameter must be a Base64 encoded string");

                errorResponse = SmtpResponse.SyntaxError;
                return false;
            }

            command = new AuthCommand(_options, method, parameter);
            return true;
        }

        /// <summary>
        /// Try to make a reverse path.
        /// </summary>
        /// <param name="mailbox">The reverse path that was made, or undefined if it was not made.</param>
        /// <returns>true if the reverse path was made, false if not.</returns>
        /// <remarks><![CDATA[Path / "<>"]]></remarks>
        public bool TryMakeReversePath(out IMailbox mailbox)
        {
            if (TryMake(TryMakePath, out mailbox))
            {
                return true;
            }

            if (Enumerator.Take() != Tokens.LessThan)
            {
                return false;
            }

            // not valid according to the spec but some senders do it
            Enumerator.Skip(TokenKind.Space);

            if (Enumerator.Take() != Tokens.GreaterThan)
            {
                return false;
            }

            mailbox = Mailbox.Empty;

            return true;
        }

        /// <summary>
        /// Try to make a path.
        /// </summary>
        /// <param name="mailbox">The path that was made, or undefined if it was not made.</param>
        /// <returns>true if the path was made, false if not.</returns>
        /// <remarks><![CDATA["<" [ A-d-l ":" ] Mailbox ">"]]></remarks>
        public bool TryMakePath(out IMailbox mailbox)
        {
            mailbox = Mailbox.Empty;

            if (Enumerator.Take() != Tokens.LessThan)
            {
                return false;
            }

            // Note, the at-domain-list must be matched, but also must be ignored
            // http://tools.ietf.org/html/rfc5321#appendix-C
            if (TryMake(TryMakeAtDomainList, out string atDomainList))
            {
                // if the @domain list was matched then it needs to be followed by a colon
                if (Enumerator.Take() != Tokens.Colon)
                {
                    return false;
                }
            }

            if (TryMake(TryMakeMailbox, out mailbox) == false)
            {
                return false;
            }

            return Enumerator.Take() == Tokens.GreaterThan;
        }

        /// <summary>
        /// Try to make an @domain list.
        /// </summary>
        /// <param name="atDomainList">The @domain list that was made, or undefined if it was not made.</param>
        /// <returns>true if the @domain list was made, false if not.</returns>
        /// <remarks><![CDATA[At-domain *( "," At-domain )]]></remarks>
        public bool TryMakeAtDomainList(out string atDomainList)
        {
            if (TryMake(TryMakeAtDomain, out atDomainList) == false)
            {
                return false;
            }

            // match the optional list
            while (Enumerator.Peek() == Tokens.Comma)
            {
                Enumerator.Take();

                if (TryMake(TryMakeAtDomain, out string atDomain) == false)
                {
                    return false;
                }

                atDomainList += $",{atDomain}";
            }

            return true;
        }

        /// <summary>
        /// Try to make an @domain.
        /// </summary>
        /// <param name="atDomain">The @domain that was made, or undefined if it was not made.</param>
        /// <returns>true if the @domain was made, false if not.</returns>
        /// <remarks><![CDATA["@" Domain]]></remarks>
        public bool TryMakeAtDomain(out string atDomain)
        {
            atDomain = null;

            if (Enumerator.Take() != Tokens.At)
            {
                return false;
            }

            if (TryMake(TryMakeDomain, out string domain) == false)
            {
                return false;
            }

            atDomain = $"@{domain}";

            return true;
        }

        /// <summary>
        /// Try to make a mailbox.
        /// </summary>
        /// <param name="mailbox">The mailbox that was made, or undefined if it was not made.</param>
        /// <returns>true if the mailbox was made, false if not.</returns>
        /// <remarks><![CDATA[Local-part "@" ( Domain / address-literal )]]></remarks>
        public bool TryMakeMailbox(out IMailbox mailbox)
        {
            mailbox = Mailbox.Empty;

            if (TryMake(TryMakeLocalPart, out string localpart) == false)
            {
                return false;
            }

            if (Enumerator.Take() != Tokens.At)
            {
                return false;
            }

            if (TryMake(TryMakeDomain, out string domain))
            {
                mailbox = new Mailbox(localpart, domain);
                return true;
            }

            if (TryMake(TryMakeAddressLiteral, out string address))
            {
                mailbox = new Mailbox(localpart, address);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Try to make a domain name.
        /// </summary>
        /// <param name="domain">The domain name that was made, or undefined if it was not made.</param>
        /// <returns>true if the domain name was made, false if not.</returns>
        /// <remarks><![CDATA[sub-domain *("." sub-domain)]]></remarks>
        public bool TryMakeDomain(out string domain)
        {
            if (TryMake(TryMakeSubdomain, out domain) == false)
            {
                return false;
            }

            while (Enumerator.Peek() == Tokens.Period)
            {
                Enumerator.Take();

                if (TryMake(TryMakeSubdomain, out string subdomain) == false)
                {
                    return false;
                }

                domain += string.Concat(".", subdomain);
            }

            return true;
        }

        /// <summary>
        /// Try to make a subdomain name.
        /// </summary>
        /// <param name="subdomain">The subdomain name that was made, or undefined if it was not made.</param>
        /// <returns>true if the subdomain name was made, false if not.</returns>
        /// <remarks><![CDATA[Let-dig [Ldh-str]]]></remarks>
        public bool TryMakeSubdomain(out string subdomain)
        {
            if (TryMake(TryMakeTextOrNumber, out subdomain) == false)
            {
                return false;
            }

            if (TryMake(TryMakeTextOrNumberOrHyphenString, out string letterNumberHyphen) == false)
            {
                return subdomain != null;
            }

            subdomain += letterNumberHyphen;

            return true;
        }

        /// <summary>
        /// Try to make a address.
        /// </summary>
        /// <param name="address">The address that was made, or undefined if it was not made.</param>
        /// <returns>true if the address was made, false if not.</returns>
        /// <remarks><![CDATA["[" ( IPv4-address-literal / IPv6-address-literal / General-address-literal ) "]"]]></remarks>
        public bool TryMakeAddressLiteral(out string address)
        {
            address = null;

            if (Enumerator.Take() != Tokens.LeftBracket)
            {
                return false;
            }

            // skip any whitespace
            Enumerator.Skip(TokenKind.Space);

            if (TryMake(TryMakeIPv4AddressLiteral, out address) == false && TryMake(TryMakeIPv6AddressLiteral, out address) == false)
            {
                return false;
            }

            // skip any whitespace
            Enumerator.Skip(TokenKind.Space);

            if (Enumerator.Take() != Tokens.RightBracket)
            {
                return false;
            }

            return address != null;
        }

        /// <summary>
        /// Try to make an IPv4 address literal.
        /// </summary>
        /// <param name="address">The address that was made, or undefined if it was not made.</param>
        /// <returns>true if the address was made, false if not.</returns>
        /// <remarks><![CDATA[ Snum 3("."  Snum) ]]></remarks>
        public bool TryMakeIPv4AddressLiteral(out string address)
        {
            address = null;

            if (TryMake(TryMakeSnum, out int snum) == false)
            {
                return false;
            }

            address = snum.ToString(CultureInfo.InvariantCulture);

            for (var i = 0; i < 3; i++)
            {
                if (Enumerator.Take() != Tokens.Period)
                {
                    return false;
                }

                if (TryMake(TryMakeSnum, out snum) == false)
                {
                    return false;
                }

                address = string.Concat(address, '.', snum);
            }

            return true;
        }

        /// <summary>
        /// Try to make an Snum (number in the range of 0-255).
        /// </summary>
        /// <param name="snum">The snum that was made, or undefined if it was not made.</param>
        /// <returns>true if the snum was made, false if not.</returns>
        /// <remarks><![CDATA[ 1*3DIGIT ]]></remarks>
        public bool TryMakeSnum(out int snum)
        {
            snum = default;

            var token = Enumerator.Take();

            if (token.Kind == TokenKind.Number && int.TryParse(token.Text, out snum))
            {
                return snum >= 0 && snum <= 255;
            }

            return false;
        }

        /// <summary>
        /// Try to make Ip version from ip version tag which is a formatted text IPv[Version]:
        /// </summary>
        /// <param name="version">IP version. IPv6 is supported atm.</param>
        /// <returns>true if ip version tag can be extracted.</returns>
        public bool TryMakeIpVersion(out int version)
        {
            version = default;
            
            if (Enumerator.Take() != Tokens.Text.IpVersionTag)
            { 
                return false;
            }

            var token = Enumerator.Take();

            if (token.Kind == TokenKind.Number && int.TryParse(token.Text, out var v))
            {
                version = v;
                return Enumerator.Take() == Tokens.Colon;
            }

            return false;
        }

        /// <summary>
        /// Try to make 16 bit hex number.
        /// </summary>
        /// <param name="hex">Extracted hex number.</param>
        /// <returns>true if valid hex number can be extracted.</returns>
        public bool TryMake16BitHex(out string hex)
        {
            hex = string.Empty;

            var token = Enumerator.Peek();
            while ((token.Kind == TokenKind.Text || token.Kind == TokenKind.Number) && hex.Length < 4)
            {
                if (token.Kind == TokenKind.Text && IsHex(token.Text) == false)
                {
                    return false;
                }

                hex = string.Concat(hex, token.Text);

                Enumerator.Take();
                token = Enumerator.Peek();
            }

            return hex.Length > 0 && hex.Length <= 4;

            bool IsHex(string text)
            {
                return text.ToUpperInvariant().All(c => c >= 'A' && c <= 'F');
            }
        }

        /// <summary>
        /// Try to extract IPv6 address. https://tools.ietf.org/html/rfc4291 section 2.2 used for specification.
        /// This method expects the address to have the IPv6: prefix.
        /// </summary>
        /// <param name="address">Extracted Ipv6 address.</param>
        /// <returns>true if a valid Ipv6 address can be extracted.</returns>
        public bool TryMakeIPv6AddressLiteral(out string address)
        {
            address = null;

            if (TryMake(TryMakeIpVersion, out int ipVersion) == false || ipVersion != 6)
            {
                return false;
            }

            return TryMakeIPv6Address(out address);
        }

        /// <summary>
        /// Try to make an IPv6 address.
        /// </summary>
        /// <param name="address">The address that was made, or undefined if it was not made.</param>
        /// <returns>true if the address was made, false if not.</returns>
        /// <remarks><![CDATA[  ]]></remarks>
        public bool TryMakeIPv6Address(out string address)
        {
            return TryMake(TryMakeIPv6AddressRule1, out address)
                || TryMake(TryMakeIPv6AddressRule2, out address)
                || TryMake(TryMakeIPv6AddressRule3, out address)
                || TryMake(TryMakeIPv6AddressRule4, out address)
                || TryMake(TryMakeIPv6AddressRule5, out address)
                || TryMake(TryMakeIPv6AddressRule6, out address)
                || TryMake(TryMakeIPv6AddressRule7, out address)
                || TryMake(TryMakeIPv6AddressRule8, out address)
                || TryMake(TryMakeIPv6AddressRule9, out address);
        }

        bool TryMakeIPv6AddressRule1(out string address)
        {
            // 6( h16 ":" ) ls32
            return TryMakeIPv6HexPostamble(6, out address);
        }

        bool TryMakeIPv6AddressRule2(out string address)
        {
            address = null;

            // "::" 5( h16 ":" ) ls32
            if (Enumerator.Take() != Tokens.Colon || Enumerator.Take() != Tokens.Colon)
            {
                return false;
            }

            if (TryMakeIPv6HexPostamble(5, out var hexPostamble) == false)
            {
                return false;
            }

            address = "::" + hexPostamble;
            return true;
        }

        bool TryMakeIPv6AddressRule3(out string address)
        {
            // [ h16 ] "::" 4( h16 ":" ) ls32
            address = null;

            if (TryMakeIPv6HexPreamble(1, out var hexPreamble) == false)
            {
                return false;
            }

            if (TryMakeIPv6HexPostamble(4, out var hexPostamble) == false)
            {
                return false;
            }

            address = hexPreamble + hexPostamble;
            return true;
        }

        bool TryMakeIPv6AddressRule4(out string address)
        {
            // [ *1( h16 ":" ) h16 ] "::" 3( h16 ":" ) ls32
            address = null;

            if (TryMakeIPv6HexPreamble(2, out var hexPreamble) == false)
            {
                return false;
            }

            if (TryMakeIPv6HexPostamble(3, out var hexPostamble) == false)
            {
                return false;
            }

            address = hexPreamble + hexPostamble;
            return true;
        }

        bool TryMakeIPv6AddressRule5(out string address)
        {
            // [ *2( h16 ":" ) h16 ] "::" 2( h16 ":" ) ls32
            address = null;

            if (TryMakeIPv6HexPreamble(3, out var hexPreamble) == false)
            {
                return false;
            }

            if (TryMakeIPv6HexPostamble(2, out var hexPostamble) == false)
            {
                return false;
            }

            address = hexPreamble + hexPostamble;
            return true;
        }

        bool TryMakeIPv6AddressRule6(out string address)
        {
            // [ *3( h16 ":" ) h16 ] "::" h16 ":" ls32
            address = null;

            if (TryMakeIPv6HexPreamble(4, out var hexPreamble) == false)
            {
                return false;
            }

            if (TryMakeIPv6HexPostamble(1, out var hexPostamble) == false)
            {
                return false;
            }

            address = hexPreamble + hexPostamble;
            return true;
        }

        bool TryMakeIPv6AddressRule7(out string address)
        {
            // [ *4( h16 ":" ) h16 ] "::" ls32
            address = null;

            if (TryMakeIPv6HexPreamble(5, out var hexPreamble) == false)
            {
                return false;
            }

            if (TryMakeIPv6Ls32(out var ls32) == false)
            {
                return false;
            }

            address = hexPreamble + ls32;
            return true;
        }

        bool TryMakeIPv6AddressRule8(out string address)
        {
            // [ *5( h16 ":" ) h16 ] "::" h16
            address = null;

            if (TryMakeIPv6HexPreamble(6, out var hexPreamble) == false)
            {
                return false;
            }
            
            if (TryMake16BitHex(out var hex) == false)
            {
                return false;
            }

            address = hexPreamble + hex;
            return true;
        }

        bool TryMakeIPv6AddressRule9(out string address)
        {
            // [ *6( h16 ":" ) h16 ] "::"
            return TryMakeIPv6HexPreamble(7, out address);
        }

        bool TryMakeIPv6HexPreamble(int maximum, out string hexPreamble)
        {
            hexPreamble = null;

            for (var i = 0; i < maximum; i++)
            {
                if (TryMake(TryMakeTerminal))
                {
                    hexPreamble += "::";
                    return true;
                }

                if (i > 0)
                {
                    if (Enumerator.Take() != Tokens.Colon)
                    {
                        return false;
                    }

                    hexPreamble += ":";
                }

                if (TryMake16BitHex(out var hex) == false)
                {
                    return false;
                }

                hexPreamble += hex;
            }

            hexPreamble += "::";

            return TryMake(TryMakeTerminal);

            bool TryMakeTerminal()
            {
                return Enumerator.Take() == Tokens.Colon && Enumerator.Take() == Tokens.Colon;
            }
        }

        bool TryMakeIPv6HexPostamble(int count, out string hexPostamble)
        {
            hexPostamble = null;

            while (count-- > 0)
            {
                if (TryMake16BitHex(out var hex) == false)
                {
                    return false;
                }

                hexPostamble += hex;

                if (Enumerator.Take() != Tokens.Colon)
                {
                    return false;
                }

                hexPostamble += ":";
            }

            if (TryMakeIPv6Ls32(out var ls32) == false)
            {
                return false;
            }

            hexPostamble += ls32;
            return true;
        }

        bool TryMakeIPv6Ls32(out string address)
        {
            if (TryMake(TryMakeIPv4AddressLiteral, out address))
            {
                return true;
            }

            if (TryMake16BitHex(out var hex1) == false)
            {
                return false;
            }

            if (Enumerator.Take() != Tokens.Colon)
            {
                return false;
            }

            if (TryMake16BitHex(out var hex2) == false)
            {
                return false;
            }

            address = hex1 + ":" + hex2;
            return true;
        }

        /// <summary>
        /// Try to make a text/number/hyphen string.
        /// </summary>
        /// <param name="textOrNumberOrHyphenString">The text, number, or hyphen that was matched, or undefined if it was not matched.</param>
        /// <returns>true if a text, number or hyphen was made, false if not.</returns>
        /// <remarks><![CDATA[*( ALPHA / DIGIT / "-" ) Let-dig]]></remarks>
        public bool TryMakeTextOrNumberOrHyphenString(out string textOrNumberOrHyphenString)
        {
            textOrNumberOrHyphenString = null;

            var token = Enumerator.Peek();
            while (token.Kind == TokenKind.Text || token.Kind == TokenKind.Number || token == Tokens.Hyphen)
            {
                textOrNumberOrHyphenString += Enumerator.Take().Text;

                token = Enumerator.Peek();
            }

            // can not end with a hyphen
            return textOrNumberOrHyphenString != null && token != Tokens.Hyphen;
        }

        /// <summary>
        /// Try to make a text or number
        /// </summary>
        /// <param name="textOrNumber">The text or number that was made, or undefined if it was not made.</param>
        /// <returns>true if the text or number was made, false if not.</returns>
        /// <remarks><![CDATA[ALPHA / DIGIT]]></remarks>
        public bool TryMakeTextOrNumber(out string textOrNumber)
        {
            var token = Enumerator.Take();

            textOrNumber = token.Text;
            
            return token.Kind == TokenKind.Text || token.Kind == TokenKind.Number;
        }

        /// <summary>
        /// Try to make the local part of the path.
        /// </summary>
        /// <param name="localPart">The local part that was made, or undefined if it was not made.</param>
        /// <returns>true if the local part was made, false if not.</returns>
        /// <remarks><![CDATA[Dot-string / Quoted-string]]></remarks>
        public bool TryMakeLocalPart(out string localPart)
        {
            if (TryMake(TryMakeDotString, out localPart))
            {
                return true;
            }

            return TryMakeQuotedString(out localPart);
        }

        /// <summary>
        /// Try to make a dot-string from the tokens.
        /// </summary>
        /// <param name="dotString">The dot-string that was made, or undefined if it was not made.</param>
        /// <returns>true if the dot-string was made, false if not.</returns>
        /// <remarks><![CDATA[Atom *("."  Atom)]]></remarks>
        public bool TryMakeDotString(out string dotString)
        {
            if (TryMake(TryMakeAtom, out dotString) == false)
            {
                return false;
            }

            while (Enumerator.Peek() == Tokens.Period)
            {
                // skip the punctuation
                Enumerator.Take();

                if (TryMake(TryMakeAtom, out string atom) == false)
                {
                    return true;
                }

                dotString += string.Concat(".", atom);
            }

            return true;
        }

        /// <summary>
        /// Try to make a quoted-string from the tokens.
        /// </summary>
        /// <param name="quotedString">The quoted-string that was made, or undefined if it was not made.</param>
        /// <returns>true if the quoted-string was made, false if not.</returns>
        /// <remarks><![CDATA[DQUOTE * QcontentSMTP DQUOTE]]></remarks>
        public bool TryMakeQuotedString(out string quotedString)
        {
            quotedString = null;

            if (Enumerator.Take() != Tokens.Quote)
            {
                return false;
            }

            while (Enumerator.Peek() != Tokens.Quote)
            {
                if (TryMakeQContentSmtp(out var text) == false)
                {
                    return false;
                }

                quotedString += text;
            }

            return Enumerator.Take() == Tokens.Quote;
        }

        /// <summary>
        /// Try to make a QcontentSMTP from the tokens.
        /// </summary>
        /// <param name="text">The text that was made.</param>
        /// <returns>true if the quoted content was made, false if not.</returns>
        /// <remarks><![CDATA[qtextSMTP / quoted-pairSMTP]]></remarks>
        public bool TryMakeQContentSmtp(out string text)
        {
            if (TryMake(TryMakeQTextSmtp, out text))
            {
                return true;
            }

            return TryMakeQuotedPairSmtp(out text);
        }

        /// <summary>
        /// Try to make a QTextSMTP from the tokens.
        /// </summary>
        /// <param name="text">The text that was made.</param>
        /// <returns>true if the quoted text was made, false if not.</returns>
        /// <remarks><![CDATA[%d32-33 / %d35-91 / %d93-126]]></remarks>
        public bool TryMakeQTextSmtp(out string text)
        {
            text = null;

            var token = Enumerator.Take();
            switch (token.Kind)
            {
                case TokenKind.Text:
                case TokenKind.Number:
                    text += token.Text;
                    return true;

                case TokenKind.Space:
                case TokenKind.Other:
                    switch (token.Text[0])
                    {
                        case ' ':
                        case '!':
                        case '#':
                        case '$':
                        case '%':
                        case '&':
                        case '\'':
                        case '(':
                        case ')':
                        case '*':
                        case '+':
                        case ',':
                        case '-':
                        case '.':
                        case '/':
                        case ':':
                        case ';':
                        case '<':
                        case '=':
                        case '>':
                        case '?':
                        case '@':
                        case '[':
                        case ']':
                        case '^':
                        case '_':
                        case '`':
                        case '{':
                        case '|':
                        case '}':
                        case '~':
                            text += token.Text[0];
                            return true;
                    }

                    return false;
            }

            return false;
        }

        /// <summary>
        /// Try to make a quoted pair from the tokens.
        /// </summary>
        /// <param name="text">The text that was made.</param>
        /// <returns>true if the quoted pair was made, false if not.</returns>
        /// <remarks><![CDATA[%d92 %d32-126]]></remarks>
        public bool TryMakeQuotedPairSmtp(out string text)
        {
            text = null;

            if (Enumerator.Take() != Tokens.BackSlash)
            {
                return false;
            }

            text += Enumerator.Take().Text;

            return true;
        }

        /// <summary>
        /// Try to make an "Atom" from the tokens.
        /// </summary>
        /// <param name="atom">The atom that was made, or undefined if it was not made.</param>
        /// <returns>true if the atom was made, false if not.</returns>
        /// <remarks><![CDATA[1*atext]]></remarks>
        public bool TryMakeAtom(out string atom)
        {
            atom = null;

            while (TryMake(TryMakeAtext, out string atext))
            {
                atom += atext;
            }

            return atom != null;
        }

        /// <summary>
        /// Try to make an "Atext" from the tokens.
        /// </summary>
        /// <param name="atext">The atext that was made, or undefined if it was not made.</param>
        /// <returns>true if the atext was made, false if not.</returns>
        /// <remarks><![CDATA[atext]]></remarks>
        public bool TryMakeAtext(out string atext)
        {
            atext = null;

            var token = Enumerator.Take();
            switch (token.Kind)
            {
                case TokenKind.Text:
                case TokenKind.Number:
                    atext = token.Text;
                    return true;

                case TokenKind.Other:
                    switch (token.Text[0])
                    {
                        case '!':
                        case '#':
                        case '%':
                        case '&':
                        case '\'':
                        case '*':
                        case '-':
                        case '/':
                        case '?':
                        case '_':
                        case '{':
                        case '}':
                        case '$':
                        case '+':
                        case '=':
                        case '^':
                        case '`':
                        case '|':
                        case '~':
                            atext += token.Text[0];
                            return true;
                    }

                    break;
            }

            return false;
        }

        /// <summary>
        /// Try to make an Mail-Parameters from the tokens.
        /// </summary>
        /// <param name="parameters">The mail parameters that were made.</param>
        /// <returns>true if the mail parameters can be made, false if not.</returns>
        /// <remarks><![CDATA[esmtp-param *(SP esmtp-param)]]></remarks>
        public bool TryMakeMailParameters(out IReadOnlyDictionary<string, string> parameters)
        {
            var dictionary = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            while (Enumerator.Peek().Kind != TokenKind.None)
            {
                if (TryMake(TryMakeEsmtpParameter, out KeyValuePair<string, string> parameter) == false)
                {
                    parameters = null;
                    return false;
                }

                dictionary.Add(parameter.Key, parameter.Value);
                Enumerator.Skip(TokenKind.Space);
            }

            parameters = dictionary;
            return parameters.Count > 0;
        }

        /// <summary>
        /// Try to make an Esmtp-Parameter from the tokens.
        /// </summary>
        /// <param name="parameter">The esmtp-parameter that was made.</param>
        /// <returns>true if the esmtp-parameter can be made, false if not.</returns>
        /// <remarks><![CDATA[esmtp-keyword ["=" esmtp-value]]]></remarks>
        public bool TryMakeEsmtpParameter(out KeyValuePair<string, string> parameter)
        {
            parameter = default;

            if (TryMake(TryMakeEsmtpKeyword, out string keyword) == false)
            {
                return false;
            }

            if (Enumerator.Peek().Kind == TokenKind.None || Enumerator.Peek().Kind == TokenKind.Space)
            {
                parameter = new KeyValuePair<string, string>(keyword, null);
                return true;
            }

            if (Enumerator.Peek() != Tokens.Equal)
            {
                return false;
            }

            Enumerator.Take();

            if (TryMake(TryMakeEsmtpValue, out string value) == false)
            {
                return false;
            }

            parameter = new KeyValuePair<string, string>(keyword, value);

            return true;
        }

        /// <summary>
        /// Try to make an Esmtp-Keyword from the tokens.
        /// </summary>
        /// <param name="keyword">The esmtp-keyword that was made.</param>
        /// <returns>true if the esmtp-keyword can be made, false if not.</returns>
        /// <remarks><![CDATA[(ALPHA / DIGIT) *(ALPHA / DIGIT / "-")]]></remarks>
        public bool TryMakeEsmtpKeyword(out string keyword)
        {
            keyword = null;

            var token = Enumerator.Peek();
            while (token.Kind == TokenKind.Text || token.Kind == TokenKind.Number || token == Tokens.Hyphen)
            {
                keyword += Enumerator.Take().Text;

                token = Enumerator.Peek();
            }

            return keyword != null;
        }

        /// <summary>
        /// Try to make an Esmtp-Value from the tokens.
        /// </summary>
        /// <param name="value">The esmtp-value that was made.</param>
        /// <returns>true if the esmtp-value can be made, false if not.</returns>
        /// <remarks><![CDATA[1*(%d33-60 / %d62-127)]]></remarks>
        public bool TryMakeEsmtpValue(out string value)
        {
            value = null;

            var token = Enumerator.Peek();
            while (token.Text.Length > 0 && token.Text.ToCharArray().All(ch => (ch >= 33 && ch <= 60) || (ch >= 62 && ch <= 127)))
            {
                value += Enumerator.Take().Text;

                token = Enumerator.Peek();
            }

            return value != null;
        }

        /// <summary>
        /// Try to make a base64 encoded string.
        /// </summary>
        /// <param name="base64">The base64 encoded string that were found.</param>
        /// <returns>true if the base64 encoded string can be made, false if not.</returns>
        /// <remarks><![CDATA[ALPHA / DIGIT / "+" / "/"]]></remarks>
        public bool TryMakeBase64(out string base64)
        {
            if (TryMakeBase64Text(out base64) == false)
            {
                return false;
            }

            if (Enumerator.Peek() == Tokens.Equal)
            {
                base64 += Enumerator.Take().Text;
            }

            if (Enumerator.Peek() == Tokens.Equal)
            {
                base64 += Enumerator.Take().Text;
            }

            // because the TryMakeBase64Chars method matches tokens, each TextValue token could make
            // up several Base64 encoded "bytes" so we ensure that we have a length divisible by 4
            return base64 != null
                && base64.Length % 4 == 0
                && new[] {TokenKind.None, TokenKind.Space, TokenKind.NewLine}.Contains(Enumerator.Peek().Kind);
        }

        /// <summary>
        /// Try to make a base64 encoded string.
        /// </summary>
        /// <param name="base64">The base64 encoded string that were found.</param>
        /// <returns>true if the base64 encoded string can be made, false if not.</returns>
        /// <remarks><![CDATA[ALPHA / DIGIT / "+" / "/"]]></remarks>
        bool TryMakeBase64Text(out string base64)
        {
            base64 = null;

            while (TryMake(TryMakeBase64Chars, out string base64Chars))
            {
                base64 += base64Chars;
            }

            return true;
        }

        /// <summary>
        /// Try to make the allowable characters in a base64 encoded string.
        /// </summary>
        /// <param name="base64Chars">The base64 characters that were found.</param>
        /// <returns>true if the base64-chars can be made, false if not.</returns>
        /// <remarks><![CDATA[ALPHA / DIGIT / "+" / "/"]]></remarks>
        bool TryMakeBase64Chars(out string base64Chars)
        {
            base64Chars = null;

            var token = Enumerator.Take();
            switch (token.Kind)
            {
                case TokenKind.Text:
                case TokenKind.Number:
                    base64Chars = token.Text;
                    return true;

                case TokenKind.Other:
                    switch (token.Text[0])
                    {
                        case '/':
                        case '+':
                            base64Chars = token.Text;
                            return true;
                    }

                    break;
            }

            return false;
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
            return string.Concat(Enumerator.Tokens.Select(token => token.Text));
        }
    }
}
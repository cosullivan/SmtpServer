using System;
using System.Buffers;
using SmtpServer.Text;

namespace SmtpServer.Protocol
{
    public sealed class SmtpParser
    {
        delegate bool TryMakeDelegate(ref TokenReader reader, out SmtpCommand command, out SmtpResponse errorResponse);

        readonly ISmtpServerOptions _options;

        public SmtpParser(ISmtpServerOptions options)
        {
            _options = options;
        }

        /// <summary>
        /// Make a command from the buffer.
        /// </summary>
        /// <param name="buffer">The buffer to read the command from.</param>
        /// <param name="command">The command that is defined within the token reader.</param>
        /// <param name="errorResponse">The error that indicates why the command could not be made.</param>
        /// <returns>Returns true if a command could be made, false if not.</returns>
        public bool TryMake(ref ReadOnlySequence<byte> buffer, out SmtpCommand command, out SmtpResponse errorResponse)
        {
            return TryMake(buffer, TryMakeEhlo, out command, out errorResponse);

            static bool TryMake(ReadOnlySequence<byte> buffer, TryMakeDelegate tryMakeDelegate, out SmtpCommand command, out SmtpResponse errorResponse)
            {
                var reader = new TokenReader(buffer);

                return tryMakeDelegate(ref reader, out command, out errorResponse);
            }
        }

        /// <summary>
        /// Make an EHLO command from the given reader.
        /// </summary>
        /// <param name="reader">The token reader to parse the command from.</param>
        /// <param name="command">The EHLO command that is defined within the token reader.</param>
        /// <param name="errorResponse">The error that indicates why the command could not be made.</param>
        /// <returns>Returns true if a command could be made, false if not.</returns>
        public bool TryMakeEhlo(ref TokenReader reader, out SmtpCommand command, out SmtpResponse errorResponse)
        {
            command = null;
            errorResponse = null;

            if (reader.TryMake(TryMakeEhlo) == false)
            {
                return false;
            }

            reader.Skip(TokenKind.Space);

            if (reader.TryMake(TryMakeDomain, out var domain))
            {
                command = new EhloCommand(_options, StringUtil.Create(domain));
                return true;
            }

            if (reader.TryMake(TryMakeAddressLiteral, out var address))
            {
                command = new EhloCommand(_options, StringUtil.Create(address));
                return true;
            }

            errorResponse = SmtpResponse.SyntaxError;
            return false;
        }

        /// <summary>
        /// Try to make the EHLO token.
        /// </summary>
        /// <param name="reader">The reader to perform the operation on.</param>
        /// <returns>true if the EHLO command could be made, false if not.</returns>
        public bool TryMakeEhlo(ref TokenReader reader)
        {
            if (reader.TryMake(TryMakeText, out var text))
            {
                Span<char> command = stackalloc char[4];
                command[0] = 'E';
                command[1] = 'H';
                command[2] = 'L';
                command[3] = 'O';

                return text.CaseInsensitiveStringEquals(ref command);
            }

            return false;
        }

        //        /// <summary>
        //        /// Try to make a reverse path.
        //        /// </summary>
        //        /// <param name="mailbox">The reverse path that was made, or undefined if it was not made.</param>
        //        /// <returns>true if the reverse path was made, false if not.</returns>
        //        /// <remarks><![CDATA[Path / "<>"]]></remarks>
        //        public bool TryMakeReversePath(out IMailbox mailbox)
        //        {
        //            if (TryMake(TryMakePath, out mailbox))
        //            {
        //                return true;
        //            }

        //            if (Enumerator.Take() != Tokens.LessThan)
        //            {
        //                return false;
        //            }

        //            // not valid according to the spec but some senders do it
        //            Enumerator.Skip(TokenKind.Space);

        //            if (Enumerator.Take() != Tokens.GreaterThan)
        //            {
        //                return false;
        //            }

        //            mailbox = Mailbox.Empty;

        //            return true;
        //        }

        //        /// <summary>
        //        /// Try to make a path.
        //        /// </summary>
        //        /// <param name="mailbox">The path that was made, or undefined if it was not made.</param>
        //        /// <returns>true if the path was made, false if not.</returns>
        //        /// <remarks><![CDATA["<" [ A-d-l ":" ] Mailbox ">"]]></remarks>
        //        public bool TryMakePath(out IMailbox mailbox)
        //        {
        //            mailbox = Mailbox.Empty;

        //            if (Enumerator.Take() != Tokens.LessThan)
        //            {
        //                return false;
        //            }

        //            // Note, the at-domain-list must be matched, but also must be ignored
        //            // http://tools.ietf.org/html/rfc5321#appendix-C
        //            if (TryMake(TryMakeAtDomainList, out string atDomainList))
        //            {
        //                // if the @domain list was matched then it needs to be followed by a colon
        //                if (Enumerator.Take() != Tokens.Colon)
        //                {
        //                    return false;
        //                }
        //            }

        //            if (TryMake(TryMakeMailbox, out mailbox) == false)
        //            {
        //                return false;
        //            }

        //            return Enumerator.Take() == Tokens.GreaterThan;
        //        }

        //        /// <summary>
        //        /// Try to make an @domain list.
        //        /// </summary>
        //        /// <param name="atDomainList">The @domain list that was made, or undefined if it was not made.</param>
        //        /// <returns>true if the @domain list was made, false if not.</returns>
        //        /// <remarks><![CDATA[At-domain *( "," At-domain )]]></remarks>
        //        public bool TryMakeAtDomainList(out string atDomainList)
        //        {
        //            if (TryMake(TryMakeAtDomain, out atDomainList) == false)
        //            {
        //                return false;
        //            }

        //            // match the optional list
        //            while (Enumerator.Peek() == Tokens.Comma)
        //            {
        //                Enumerator.Take();

        //                if (TryMake(TryMakeAtDomain, out string atDomain) == false)
        //                {
        //                    return false;
        //                }

        //                atDomainList += $",{atDomain}";
        //            }

        //            return true;
        //        }

        ///// <summary>
        ///// Try to make an @domain.
        ///// </summary>
        ///// <param name="atDomain">The @domain that was made, or undefined if it was not made.</param>
        ///// <returns>true if the @domain was made, false if not.</returns>
        ///// <remarks><![CDATA["@" Domain]]></remarks>
        //public bool TryMakeAtDomain(out string atDomain)
        //{
        //    atDomain = null;

        //    if (Enumerator.Take() != Tokens.At)
        //    {
        //        return false;
        //    }

        //    if (TryMake(TryMakeDomain, out string domain) == false)
        //    {
        //        return false;
        //    }

        //    atDomain = $"@{domain}";

        //    return true;
        //}

        //        /// <summary>
        //        /// Try to make a mailbox.
        //        /// </summary>
        //        /// <param name="mailbox">The mailbox that was made, or undefined if it was not made.</param>
        //        /// <returns>true if the mailbox was made, false if not.</returns>
        //        /// <remarks><![CDATA[Local-part "@" ( Domain / address-literal )]]></remarks>
        //        public bool TryMakeMailbox(out IMailbox mailbox)
        //        {
        //            mailbox = Mailbox.Empty;

        //            if (TryMake(TryMakeLocalPart, out string localpart) == false)
        //            {
        //                return false;
        //            }

        //            if (Enumerator.Take() != Tokens.At)
        //            {
        //                return false;
        //            }

        //            if (TryMake(TryMakeDomain, out string domain))
        //            {
        //                mailbox = new Mailbox(localpart, domain);
        //                return true;
        //            }

        //            if (TryMake(TryMakeAddressLiteral, out string address))
        //            {
        //                mailbox = new Mailbox(localpart, address);
        //                return true;
        //            }

        //            return false;
        //        }

        /// <summary>
        /// Try to make a domain name.
        /// </summary>
        /// <param name="reader">The reader to perform the operation on.</param>
        /// <returns>true if the domain name was made, false if not.</returns>
        /// <remarks><![CDATA[sub-domain *("." sub-domain)]]></remarks>
        public bool TryMakeDomain(ref TokenReader reader)
        {
            if (TryMakeSubdomain(ref reader) == false)
            {
                return false;
            }

            while (reader.Peek().Kind == TokenKind.Period)
            {
                reader.Take();

                if (TryMakeSubdomain(ref reader) == false)
                {
                    return false;
                }
            }
            
            return true;
        }

        /// <summary>
        /// Try to make a subdomain name.
        /// </summary>
        /// <param name="reader">The reader to perform the operation on.</param>
        /// <returns>true if the subdomain name was made, false if not.</returns>
        /// <remarks><![CDATA[Let-dig [Ldh-str]]]></remarks>
        public bool TryMakeSubdomain(ref TokenReader reader)
        {
            if (TryMakeTextOrNumber(ref reader) == false)
            {
                return false;
            }

            // this is optional
            reader.TryMake(TryMakeTextOrNumberOrHyphenString);

            return true;
        }

        /// <summary>
        /// Try to make a address.
        /// </summary>
        /// <param name="reader">The reader to perform the operation on.</param>
        /// <returns>true if the address was made, false if not.</returns>
        /// <remarks><![CDATA["[" ( IPv4-address-literal / IPv6-address-literal / General-address-literal ) "]"]]></remarks>
        public bool TryMakeAddressLiteral(ref TokenReader reader)
        {
            if (reader.Take().Kind != TokenKind.LeftBracket)
            {
                return false;
            }

            reader.Skip(TokenKind.Space);

            if (reader.TryMake(TryMakeIPv4AddressLiteral) == false && reader.TryMake(TryMakeIPv6AddressLiteral) == false)
            {
                return false;
            }

            reader.Skip(TokenKind.Space);

            return reader.Take().Kind == TokenKind.RightBracket;
        }

        /// <summary>
        /// Try to make an IPv4 address literal.
        /// </summary>
        /// <param name="reader">The reader to perform the operation on.</param>
        /// <returns>true if the address was made, false if not.</returns>
        /// <remarks><![CDATA[ Snum 3("."  Snum) ]]></remarks>
        public bool TryMakeIPv4AddressLiteral(ref TokenReader reader)
        {
            if (reader.TryMake(TryMakeSnum) == false)
            {
                return false;
            }

            for (var i = 0; i < 3; i++)
            {
                if (reader.Take().Kind != TokenKind.Period)
                {
                    return false;
                }

                if (reader.TryMake(TryMakeSnum) == false)
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Try to make an Snum (number in the range of 0-255).
        /// </summary>
        /// <param name="reader">The reader to perform the operation on.</param>
        /// <returns>true if the snum was made, false if not.</returns>
        /// <remarks><![CDATA[ 1*3DIGIT ]]></remarks>
        public bool TryMakeSnum(ref TokenReader reader)
        {
            if (reader.TryMake(TryMakeNumber, out var number) == false)
            {
                return false;
            }

            return int.TryParse(StringUtil.Create(number), out var snum) && snum >= 0 && snum <= 255;
        }

        ///// <summary>
        ///// Try to make Ip version from ip version tag which is a formatted text IPv[Version]:
        ///// </summary>
        ///// <param name="reader">The reader to perform the operation on.</param>
        ///// <returns>true if ip version tag can be extracted.</returns>
        //public bool TryMakeIpVersion(ref TokenReader reader)
        //{
        //    //version = default;

        //    //if (Enumerator.Take() != Tokens.Text.IpVersionTag)
        //    //{
        //    //    return false;
        //    //}

        //    //var token = Enumerator.Take();

        //    //if (token.Kind == TokenKind.Number && int.TryParse(token.Text, out var v))
        //    //{
        //    //    version = v;
        //    //    return Enumerator.Take() == Tokens.Colon;
        //    //}

        //    //return false;
        //    throw new NotImplementedException();
        //}

        /// <summary>
        /// Try to extract IPv6 address. https://tools.ietf.org/html/rfc4291 section 2.2 used for specification.
        /// This method expects the address to have the IPv6: prefix.
        /// </summary>
        /// <param name="reader">The reader to perform the operation on.</param>
        /// <returns>true if a valid Ipv6 address can be extracted.</returns>
        public bool TryMakeIPv6AddressLiteral(ref TokenReader reader)
        {
            if (TryMakeIPv6(ref reader) == false)
            {
                return false;
            }

            return TryMakeIPv6Address(ref reader);
        }

        /// <summary>
        /// Try to make Ip version from ip version tag which is a formatted text IPv[Version]:
        /// </summary>
        /// <param name="reader">The reader to perform the operation on.</param>
        /// <returns>true if ip version tag can be extracted.</returns>
        public bool TryMakeIPv6(ref TokenReader reader)
        {
            if (TryMakeIPv(ref reader) == false)
            {
                return false;
            }

            var token = reader.Take();

            if (token.Kind != TokenKind.Number || token.Text.Length > 1)
            {
                return false;
            }

            return token.Text[0] == '6';
        }

        /// <summary>
        /// Try to make the IPv text sequence.
        /// </summary>
        /// <param name="reader">The reader to perform the operation on.</param>
        /// <returns>true if IPv text sequence can be made.</returns>
        public bool TryMakeIPv(ref TokenReader reader)
        {
            if (reader.TryMake(TryMakeText, out var text))
            {
                Span<char> command = stackalloc char[3];
                command[0] = 'I';
                command[1] = 'P';
                command[2] = 'v';

                return text.CaseInsensitiveStringEquals(ref command);
            }

            return false;
        }

        /// <summary>
        /// Try to make an IPv6 address.
        /// </summary>
        /// <param name="reader">The reader to perform the operation on.</param>
        /// <returns>true if the address was made, false if not.</returns>
        /// <remarks><![CDATA[  ]]></remarks>
        public bool TryMakeIPv6Address(ref TokenReader reader)
        {
            return reader.TryMake(TryMakeIPv6AddressRule1)
                || reader.TryMake(TryMakeIPv6AddressRule2)
                || reader.TryMake(TryMakeIPv6AddressRule3)
                || reader.TryMake(TryMakeIPv6AddressRule4)
                || reader.TryMake(TryMakeIPv6AddressRule5)
                || reader.TryMake(TryMakeIPv6AddressRule6)
                || reader.TryMake(TryMakeIPv6AddressRule7)
                || reader.TryMake(TryMakeIPv6AddressRule8)
                || reader.TryMake(TryMakeIPv6AddressRule9);
        }

        public bool TryMakeIPv6AddressRule1(ref TokenReader reader)
        {
            // 6( h16 ":" ) ls32
            return TryMakeIPv6HexPostamble(ref reader, 6);
        }

        bool TryMakeIPv6AddressRule2(ref TokenReader reader)
        {
            // "::" 5( h16 ":" ) ls32
            if (reader.Take().Kind != TokenKind.Colon || reader.Take().Kind != TokenKind.Colon)
            {
                return false;
            }

            return TryMakeIPv6HexPostamble(ref reader, 5);
        }

        bool TryMakeIPv6AddressRule3(ref TokenReader reader)
        {
            // [ h16 ] "::" 4( h16 ":" ) ls32
            if (TryMakeIPv6HexPreamble(ref reader, 1) == false)
            {
                return false;
            }

            return TryMakeIPv6HexPostamble(ref reader, 4);
        }

        bool TryMakeIPv6AddressRule4(ref TokenReader reader)
        {
            // [ *1( h16 ":" ) h16 ] "::" 3( h16 ":" ) ls32
            if (TryMakeIPv6HexPreamble(ref reader, 2) == false)
            {
                return false;
            }

            return TryMakeIPv6HexPostamble(ref reader, 3);
        }

        bool TryMakeIPv6AddressRule5(ref TokenReader reader)
        {
            // [ *2( h16 ":" ) h16 ] "::" 2( h16 ":" ) ls32
            if (TryMakeIPv6HexPreamble(ref reader, 3) == false)
            {
                return false;
            }

            return TryMakeIPv6HexPostamble(ref reader, 2);
        }

        bool TryMakeIPv6AddressRule6(ref TokenReader reader)
        {
            // [ *3( h16 ":" ) h16 ] "::" h16 ":" ls32
            if (TryMakeIPv6HexPreamble(ref reader, 4) == false)
            {
                return false;
            }

            return TryMakeIPv6HexPostamble(ref reader, 1);
        }

        bool TryMakeIPv6AddressRule7(ref TokenReader reader)
        {
            // [ *4( h16 ":" ) h16 ] "::" ls32
            if (TryMakeIPv6HexPreamble(ref reader, 5) == false)
            {
                return false;
            }

            return TryMakeIPv6Ls32(ref reader);
        }

        bool TryMakeIPv6AddressRule8(ref TokenReader reader)
        {
            // [ *5( h16 ":" ) h16 ] "::" h16
            if (TryMakeIPv6HexPreamble(ref reader, 6) == false)
            {
                return false;
            }

            return TryMake16BitHex(ref reader);
        }

        bool TryMakeIPv6AddressRule9(ref TokenReader reader)
        {
            // [ *6( h16 ":" ) h16 ] "::"
            return TryMakeIPv6HexPreamble(ref reader, 7);
        }

        bool TryMakeIPv6HexPreamble(ref TokenReader reader, int maximum)
        {
            for (var i = 0; i < maximum; i++)
            {
                if (reader.TryMake(TryMakeTerminal))
                {
                    return true;
                }

                if (i > 0)
                {
                    if (reader.Take().Kind != TokenKind.Colon)
                    {
                        return false;
                    }
                }

                if (TryMake16BitHex(ref reader) == false)
                {
                    return false;
                }
            }

            return reader.TryMake(TryMakeTerminal);

            static bool TryMakeTerminal(ref TokenReader reader)
            {
                return reader.Take().Kind == TokenKind.Colon && reader.Take().Kind == TokenKind.Colon;
            }
        }

        bool TryMakeIPv6HexPostamble(ref TokenReader reader, int count)
        {
            while (count-- > 0)
            {
                if (TryMake16BitHex(ref reader) == false)
                {
                    return false;
                }

                if (reader.Take().Kind != TokenKind.Colon)
                {
                    return false;
                }
            }

            return TryMakeIPv6Ls32(ref reader);
        }

        bool TryMakeIPv6Ls32(ref TokenReader reader)
        {
            if (reader.TryMake(TryMakeIPv4AddressLiteral))
            {
                return true;
            }

            if (TryMake16BitHex(ref reader) == false)
            {
                return false;
            }

            if (reader.Take().Kind != TokenKind.Colon)
            {
                return false;
            }

            return TryMake16BitHex(ref reader);
        }

        /// <summary>
        /// Try to make 16 bit hex number.
        /// </summary>
        /// <param name="reader">The token reader to perform the operation on.</param>
        /// <returns>true if valid hex number can be extracted.</returns>
        public bool TryMake16BitHex(ref TokenReader reader)
        {
            var hexLength = 0L;

            var token = reader.Peek();
            while ((token.Kind == TokenKind.Text || token.Kind == TokenKind.Number) && hexLength < 4)
            {
                if (token.Kind == TokenKind.Text && IsHex(ref token) == false)
                {
                    return false;
                }

                hexLength += reader.Take().Text.Length;

                token = reader.Peek();
            }

            return hexLength > 0 && hexLength <= 4;

            static bool IsHex(ref Token token)
            {
                var span = token.Text;

                return span.IsHex();
            }
        }

        /// <summary>
        /// Try to make a text/number/hyphen string.
        /// </summary>
        /// <param name="reader">The reader to perform the operatio on.</param>
        /// <returns>true if a text, number or hyphen was made, false if not.</returns>
        /// <remarks><![CDATA[*( ALPHA / DIGIT / "-" ) Let-dig]]></remarks>
        public bool TryMakeTextOrNumberOrHyphenString(ref TokenReader reader)
        {
            var token = reader.Peek();

            if (token.Kind == TokenKind.Text || token.Kind == TokenKind.Number || token.Kind == TokenKind.Hyphen)
            {
                reader.Skip(kind => kind == TokenKind.Text || kind == TokenKind.Number || kind == TokenKind.Hyphen);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Try to make a text or number
        /// </summary>
        /// <param name="reader">The reader to perform the operatio on.</param>
        /// <returns>true if the text or number was made, false if not.</returns>
        /// <remarks><![CDATA[ALPHA / DIGIT]]></remarks>
        public bool TryMakeTextOrNumber(ref TokenReader reader)
        {
            var token = reader.Peek();
            
            if (token.Kind == TokenKind.Text)
            {
                return TryMakeText(ref reader);
            }

            if (token.Kind == TokenKind.Number)
            {
                return TryMakeNumber(ref reader);
            }

            return false;
        }

        //        /// <summary>
        //        /// Try to make the local part of the path.
        //        /// </summary>
        //        /// <param name="localPart">The local part that was made, or undefined if it was not made.</param>
        //        /// <returns>true if the local part was made, false if not.</returns>
        //        /// <remarks><![CDATA[Dot-string / Quoted-string]]></remarks>
        //        public bool TryMakeLocalPart(out string localPart)
        //        {
        //            if (TryMake(TryMakeDotString, out localPart))
        //            {
        //                return true;
        //            }

        //            return TryMakeQuotedString(out localPart);
        //        }

        //        /// <summary>
        //        /// Try to make a dot-string from the tokens.
        //        /// </summary>
        //        /// <param name="dotString">The dot-string that was made, or undefined if it was not made.</param>
        //        /// <returns>true if the dot-string was made, false if not.</returns>
        //        /// <remarks><![CDATA[Atom *("."  Atom)]]></remarks>
        //        public bool TryMakeDotString(out string dotString)
        //        {
        //            if (TryMake(TryMakeAtom, out dotString) == false)
        //            {
        //                return false;
        //            }

        //            while (Enumerator.Peek() == Tokens.Period)
        //            {
        //                // skip the punctuation
        //                Enumerator.Take();

        //                if (TryMake(TryMakeAtom, out string atom) == false)
        //                {
        //                    return true;
        //                }

        //                dotString += string.Concat(".", atom);
        //            }

        //            return true;
        //        }

        //        /// <summary>
        //        /// Try to make a quoted-string from the tokens.
        //        /// </summary>
        //        /// <param name="quotedString">The quoted-string that was made, or undefined if it was not made.</param>
        //        /// <returns>true if the quoted-string was made, false if not.</returns>
        //        /// <remarks><![CDATA[DQUOTE * QcontentSMTP DQUOTE]]></remarks>
        //        public bool TryMakeQuotedString(out string quotedString)
        //        {
        //            quotedString = null;

        //            if (Enumerator.Take() != Tokens.Quote)
        //            {
        //                return false;
        //            }

        //            while (Enumerator.Peek() != Tokens.Quote)
        //            {
        //                if (TryMakeQContentSmtp(out var text) == false)
        //                {
        //                    return false;
        //                }

        //                quotedString += text;
        //            }

        //            return Enumerator.Take() == Tokens.Quote;
        //        }

        //        /// <summary>
        //        /// Try to make a QcontentSMTP from the tokens.
        //        /// </summary>
        //        /// <param name="text">The text that was made.</param>
        //        /// <returns>true if the quoted content was made, false if not.</returns>
        //        /// <remarks><![CDATA[qtextSMTP / quoted-pairSMTP]]></remarks>
        //        public bool TryMakeQContentSmtp(out string text)
        //        {
        //            if (TryMake(TryMakeQTextSmtp, out text))
        //            {
        //                return true;
        //            }

        //            return TryMakeQuotedPairSmtp(out text);
        //        }

        //        /// <summary>
        //        /// Try to make a QTextSMTP from the tokens.
        //        /// </summary>
        //        /// <param name="text">The text that was made.</param>
        //        /// <returns>true if the quoted text was made, false if not.</returns>
        //        /// <remarks><![CDATA[%d32-33 / %d35-91 / %d93-126]]></remarks>
        //        public bool TryMakeQTextSmtp(out string text)
        //        {
        //            text = null;

        //            var token = Enumerator.Take();
        //            switch (token.Kind)
        //            {
        //                case TokenKind.Text:
        //                case TokenKind.Number:
        //                    text += token.Text;
        //                    return true;

        //                case TokenKind.Space:
        //                case TokenKind.Other:
        //                    switch (token.Text[0])
        //                    {
        //                        case ' ':
        //                        case '!':
        //                        case '#':
        //                        case '$':
        //                        case '%':
        //                        case '&':
        //                        case '\'':
        //                        case '(':
        //                        case ')':
        //                        case '*':
        //                        case '+':
        //                        case ',':
        //                        case '-':
        //                        case '.':
        //                        case '/':
        //                        case ':':
        //                        case ';':
        //                        case '<':
        //                        case '=':
        //                        case '>':
        //                        case '?':
        //                        case '@':
        //                        case '[':
        //                        case ']':
        //                        case '^':
        //                        case '_':
        //                        case '`':
        //                        case '{':
        //                        case '|':
        //                        case '}':
        //                        case '~':
        //                            text += token.Text[0];
        //                            return true;
        //                    }

        //                    return false;
        //            }

        //            return false;
        //        }

        //        /// <summary>
        //        /// Try to make a quoted pair from the tokens.
        //        /// </summary>
        //        /// <param name="text">The text that was made.</param>
        //        /// <returns>true if the quoted pair was made, false if not.</returns>
        //        /// <remarks><![CDATA[%d92 %d32-126]]></remarks>
        //        public bool TryMakeQuotedPairSmtp(out string text)
        //        {
        //            text = null;

        //            if (Enumerator.Take() != Tokens.BackSlash)
        //            {
        //                return false;
        //            }

        //            text += Enumerator.Take().Text;

        //            return true;
        //        }

        //        /// <summary>
        //        /// Try to make an "Atom" from the tokens.
        //        /// </summary>
        //        /// <param name="atom">The atom that was made, or undefined if it was not made.</param>
        //        /// <returns>true if the atom was made, false if not.</returns>
        //        /// <remarks><![CDATA[1*atext]]></remarks>
        //        public bool TryMakeAtom(out string atom)
        //        {
        //            atom = null;

        //            while (TryMake(TryMakeAtext, out string atext))
        //            {
        //                atom += atext;
        //            }

        //            return atom != null;
        //        }

        //        /// <summary>
        //        /// Try to make an "Atext" from the tokens.
        //        /// </summary>
        //        /// <param name="atext">The atext that was made, or undefined if it was not made.</param>
        //        /// <returns>true if the atext was made, false if not.</returns>
        //        /// <remarks><![CDATA[atext]]></remarks>
        //        public bool TryMakeAtext(out string atext)
        //        {
        //            atext = null;

        //            var token = Enumerator.Take();
        //            switch (token.Kind)
        //            {
        //                case TokenKind.Text:
        //                case TokenKind.Number:
        //                    atext = token.Text;
        //                    return true;

        //                case TokenKind.Other:
        //                    switch (token.Text[0])
        //                    {
        //                        case '!':
        //                        case '#':
        //                        case '%':
        //                        case '&':
        //                        case '\'':
        //                        case '*':
        //                        case '-':
        //                        case '/':
        //                        case '?':
        //                        case '_':
        //                        case '{':
        //                        case '}':
        //                        case '$':
        //                        case '+':
        //                        case '=':
        //                        case '^':
        //                        case '`':
        //                        case '|':
        //                        case '~':
        //                            atext += token.Text[0];
        //                            return true;
        //                    }

        //                    break;
        //            }

        //            return false;
        //        }

        //        /// <summary>
        //        /// Try to make an Mail-Parameters from the tokens.
        //        /// </summary>
        //        /// <param name="parameters">The mail parameters that were made.</param>
        //        /// <returns>true if the mail parameters can be made, false if not.</returns>
        //        /// <remarks><![CDATA[esmtp-param *(SP esmtp-param)]]></remarks>
        //        public bool TryMakeMailParameters(out IReadOnlyDictionary<string, string> parameters)
        //        {
        //            var dictionary = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        //            while (Enumerator.Peek().Kind != TokenKind.None)
        //            {
        //                if (TryMake(TryMakeEsmtpParameter, out KeyValuePair<string, string> parameter) == false)
        //                {
        //                    parameters = null;
        //                    return false;
        //                }

        //                dictionary.Add(parameter.Key, parameter.Value);
        //                Enumerator.Skip(TokenKind.Space);
        //            }

        //            parameters = dictionary;
        //            return parameters.Count > 0;
        //        }

        //        /// <summary>
        //        /// Try to make an Esmtp-Parameter from the tokens.
        //        /// </summary>
        //        /// <param name="parameter">The esmtp-parameter that was made.</param>
        //        /// <returns>true if the esmtp-parameter can be made, false if not.</returns>
        //        /// <remarks><![CDATA[esmtp-keyword ["=" esmtp-value]]]></remarks>
        //        public bool TryMakeEsmtpParameter(out KeyValuePair<string, string> parameter)
        //        {
        //            parameter = default;

        //            if (TryMake(TryMakeEsmtpKeyword, out string keyword) == false)
        //            {
        //                return false;
        //            }

        //            if (Enumerator.Peek().Kind == TokenKind.None || Enumerator.Peek().Kind == TokenKind.Space)
        //            {
        //                parameter = new KeyValuePair<string, string>(keyword, null);
        //                return true;
        //            }

        //            if (Enumerator.Peek() != Tokens.Equal)
        //            {
        //                return false;
        //            }

        //            Enumerator.Take();

        //            if (TryMake(TryMakeEsmtpValue, out string value) == false)
        //            {
        //                return false;
        //            }

        //            parameter = new KeyValuePair<string, string>(keyword, value);

        //            return true;
        //        }

        //        /// <summary>
        //        /// Try to make an Esmtp-Keyword from the tokens.
        //        /// </summary>
        //        /// <param name="keyword">The esmtp-keyword that was made.</param>
        //        /// <returns>true if the esmtp-keyword can be made, false if not.</returns>
        //        /// <remarks><![CDATA[(ALPHA / DIGIT) *(ALPHA / DIGIT / "-")]]></remarks>
        //        public bool TryMakeEsmtpKeyword(out string keyword)
        //        {
        //            keyword = null;

        //            var token = Enumerator.Peek();
        //            while (token.Kind == TokenKind.Text || token.Kind == TokenKind.Number || token == Tokens.Hyphen)
        //            {
        //                keyword += Enumerator.Take().Text;

        //                token = Enumerator.Peek();
        //            }

        //            return keyword != null;
        //        }

        //        /// <summary>
        //        /// Try to make an Esmtp-Value from the tokens.
        //        /// </summary>
        //        /// <param name="value">The esmtp-value that was made.</param>
        //        /// <returns>true if the esmtp-value can be made, false if not.</returns>
        //        /// <remarks><![CDATA[1*(%d33-60 / %d62-127)]]></remarks>
        //        public bool TryMakeEsmtpValue(out string value)
        //        {
        //            value = null;

        //            var token = Enumerator.Peek();
        //            while (token.Text.Length > 0 && token.Text.ToCharArray().All(ch => (ch >= 33 && ch <= 60) || (ch >= 62 && ch <= 127)))
        //            {
        //                value += Enumerator.Take().Text;

        //                token = Enumerator.Peek();
        //            }

        //            return value != null;
        //        }

        //        /// <summary>
        //        /// Try to make a base64 encoded string.
        //        /// </summary>
        //        /// <param name="base64">The base64 encoded string that were found.</param>
        //        /// <returns>true if the base64 encoded string can be made, false if not.</returns>
        //        /// <remarks><![CDATA[ALPHA / DIGIT / "+" / "/"]]></remarks>
        //        public bool TryMakeBase64(out string base64)
        //        {
        //            if (TryMakeBase64Text(out base64) == false)
        //            {
        //                return false;
        //            }

        //            if (Enumerator.Peek() == Tokens.Equal)
        //            {
        //                base64 += Enumerator.Take().Text;
        //            }

        //            if (Enumerator.Peek() == Tokens.Equal)
        //            {
        //                base64 += Enumerator.Take().Text;
        //            }

        //            // because the TryMakeBase64Chars method matches tokens, each TextValue token could make
        //            // up several Base64 encoded "bytes" so we ensure that we have a length divisible by 4
        //            return base64 != null
        //                && base64.Length % 4 == 0
        //                && new[] {TokenKind.None, TokenKind.Space, TokenKind.NewLine}.Contains(Enumerator.Peek().Kind);
        //        }

        //        /// <summary>
        //        /// Try to make a base64 encoded string.
        //        /// </summary>
        //        /// <param name="base64">The base64 encoded string that were found.</param>
        //        /// <returns>true if the base64 encoded string can be made, false if not.</returns>
        //        /// <remarks><![CDATA[ALPHA / DIGIT / "+" / "/"]]></remarks>
        //        bool TryMakeBase64Text(out string base64)
        //        {
        //            base64 = null;

        //            while (TryMake(TryMakeBase64Chars, out string base64Chars))
        //            {
        //                base64 += base64Chars;
        //            }

        //            return true;
        //        }

        //        /// <summary>
        //        /// Try to make the allowable characters in a base64 encoded string.
        //        /// </summary>
        //        /// <param name="base64Chars">The base64 characters that were found.</param>
        //        /// <returns>true if the base64-chars can be made, false if not.</returns>
        //        /// <remarks><![CDATA[ALPHA / DIGIT / "+" / "/"]]></remarks>
        //        bool TryMakeBase64Chars(out string base64Chars)
        //        {
        //            base64Chars = null;

        //            var token = Enumerator.Take();
        //            switch (token.Kind)
        //            {
        //                case TokenKind.Text:
        //                case TokenKind.Number:
        //                    base64Chars = token.Text;
        //                    return true;

        //                case TokenKind.Other:
        //                    switch (token.Text[0])
        //                    {
        //                        case '/':
        //                        case '+':
        //                            base64Chars = token.Text;
        //                            return true;
        //                    }

        //                    break;
        //            }

        //            return false;
        //        }

        //        /// <summary>
        //        /// Attempt to make the end of the line.
        //        /// </summary>
        //        /// <returns>true if the end of the line could be made, false if not.</returns>
        //        bool TryMakeEnd()
        //        {
        //            Enumerator.Skip(TokenKind.Space);

        //            return Enumerator.Take() == Token.None;
        //        }


        /// <summary>
        /// Try to make a text sequence.
        /// </summary>
        /// <param name="reader">The reader to perform the operation on.</param>
        /// <returns>true if a text sequence could be made, false if not.</returns>
        public bool TryMakeText(ref TokenReader reader)
        {
            if (reader.Peek().Kind == TokenKind.Text)
            {
                reader.Skip(TokenKind.Text);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Try to make a number sequence.
        /// </summary>
        /// <param name="reader">The reader to perform the operation on.</param>
        /// <returns>true if a number sequence could be made, false if not.</returns>
        public bool TryMakeNumber(ref TokenReader reader)
        {
            if (reader.Peek().Kind == TokenKind.Number)
            {
                reader.Skip(TokenKind.Number);
                return true;
            }

            return false;
        }
    }
}
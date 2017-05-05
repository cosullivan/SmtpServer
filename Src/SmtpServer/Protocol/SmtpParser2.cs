using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using SmtpServer.Mail;
using SmtpServer.Text;

namespace SmtpServer.Protocol
{
    /// <remarks>
    /// This class is responsible for parsing the SMTP command arguments according to the ANBF described in
    /// the RFC http://tools.ietf.org/html/rfc5321#section-4.1.2
    /// </remarks>
    public class SmtpParser2 : TokenParser
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="enumerator">The token enumerator to handle the incoming tokens.</param>
        public SmtpParser2(ITokenEnumerator enumerator) : base(enumerator) { }

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

            if (Enumerator.Take() != new Token(TokenKind.Other, "<"))
            {
                return false;
            }

            // not valid according to the spec but some senders do it
            Enumerator.Skip(TokenKind.Space);

            if (Enumerator.Take() != new Token(TokenKind.Other, ">"))
            {
                return false;
            }

            mailbox = null;

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
            mailbox = null;

            if (Enumerator.Take() != new Token(TokenKind.Other, "<"))
            {
                return false;
            }

            // Note, the at-domain-list must be matched, but also must be ignored
            // http://tools.ietf.org/html/rfc5321#appendix-C
            string atDomainList;
            if (TryMake(TryMakeAtDomainList, out atDomainList))
            {
                // if the @domain list was matched then it needs to be followed by a colon
                if (Enumerator.Take() != new Token(TokenKind.Other, ":"))
                {
                    return false;
                }
            }

            if (TryMake(TryMakeMailbox, out mailbox) == false)
            {
                return false;
            }

            return Enumerator.Take() == new Token(TokenKind.Other, ">");
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
            while (Enumerator.Peek() == new Token(TokenKind.Other, ","))
            {
                Enumerator.Take();

                string atDomain;
                if (TryMake(TryMakeAtDomain, out atDomain) == false)
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

            if (Enumerator.Take() != new Token(TokenKind.Other, "@"))
            {
                return false;
            }

            string domain;
            if (TryMake(TryMakeDomain, out domain) == false)
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
            mailbox = null;

            string localpart;
            if (TryMake(TryMakeLocalPart, out localpart) == false)
            {
                return false;
            }

            if (Enumerator.Take() != new Token(TokenKind.Other, "@"))
            {
                return false;
            }

            string domain;
            if (TryMake(TryMakeDomain, out domain))
            {
                mailbox = new Mailbox(localpart, domain);

                return true;
            }

            string address;
            if (TryMake(TryMakeAddressLiteral, out address))
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

            while (Enumerator.Peek() == new Token(TokenKind.Other, "."))
            {
                Enumerator.Take();

                string subdomain;
                if (TryMake(TryMakeSubdomain, out subdomain) == false)
                {
                    return false;
                }

                domain += String.Concat(".", subdomain);
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

            string letterNumberHyphen;
            if (TryMake(TryMakeTextOrNumberOrHyphenString, out letterNumberHyphen) == false)
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

            if (Enumerator.Take() != new Token(TokenKind.Other, "["))
            {
                return false;
            }

            // skip any whitespace
            Enumerator.Skip(TokenKind.Space);

            if (TryMake(TryMakeIpv4AddressLiteral, out address) == false)
            {
                return false;
            }

            // skip any whitespace
            Enumerator.Skip(TokenKind.Space);

            if (Enumerator.Take() != new Token(TokenKind.Other, "]"))
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
        public bool TryMakeIpv4AddressLiteral(out string address)
        {
            address = null;

            int snum;
            if (TryMake(TryMakeSnum, out snum) == false)
            {
                return false;
            }

            address = snum.ToString(CultureInfo.InvariantCulture);

            for (var i = 0; i < 3 && Enumerator.Peek() == new Token(TokenKind.Other, "."); i++)
            {
                Enumerator.Take();

                if (TryMake(TryMakeSnum, out snum) == false)
                {
                    return false;
                }

                address = String.Concat(address, '.', snum);
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
            var token = Enumerator.Take();

            if (Int32.TryParse(token.Text, out snum) && token.Kind == TokenKind.Number)
            {
                return snum >= 0 && snum <= 255;
            }

            return false;
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
            while (token.Kind == TokenKind.Text || token.Kind == TokenKind.Number || token == new Token(TokenKind.Other, "-"))
            {
                textOrNumberOrHyphenString += Enumerator.Take().Text;

                token = Enumerator.Peek();
            }

            // can not end with a hyphen
            return textOrNumberOrHyphenString != null && token != new Token(TokenKind.Other, "-");
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

            while (Enumerator.Peek() == new Token(TokenKind.Other, "."))
            {
                // skip the punctuation
                Enumerator.Take();

                string atom;
                if (TryMake(TryMakeAtom, out atom) == false)
                {
                    return true;
                }

                dotString += String.Concat(".", atom);
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

            if (Enumerator.Take() != new Token(TokenKind.Other, "\""))
            {
                return false;
            }

            while (Enumerator.Peek() != new Token(TokenKind.Other, "\""))
            {
                string text;
                if (TryMakeQContentSmtp(out text) == false)
                {
                    return false;
                }

                quotedString += text;
            }

            return Enumerator.Take() == new Token(TokenKind.Other, "\"");
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

            if (Enumerator.Take() != new Token(TokenKind.Other, '\\'))
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

            string atext;
            while (TryMake(TryMakeAtext, out atext))
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
                            atext = token.Text;
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
                KeyValuePair<string, string> parameter;
                if (TryMake(TryMakeEsmtpParameter, out parameter) == false)
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
            parameter = default(KeyValuePair<string, string>);

            string keyword;
            if (TryMake(TryMakeEsmtpKeyword, out keyword) == false)
            {
                return false;
            }

            if (Enumerator.Peek().Kind == TokenKind.None || Enumerator.Peek().Kind == TokenKind.Space)
            {
                parameter = new KeyValuePair<string, string>(keyword, null);
                return true;
            }

            if (Enumerator.Peek() != new Token(TokenKind.Other, "="))
            {
                return false;
            }

            Enumerator.Take();

            string value;
            if (TryMake(TryMakeEsmtpValue, out value) == false)
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
            while (token.Kind == TokenKind.Text || token.Kind == TokenKind.Number || token == new Token(TokenKind.Other, "-"))
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
            while (token.Text.Length > 0 && token.Text.ToCharArray().All(ch => (ch >= 33 && ch <= 66) || (ch >= 62 && ch <= 127)))
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
            base64 = null;

            while (Enumerator.Peek().Kind != TokenKind.None)
            {
                string base64Chars;
                if (TryMake(TryMakeBase64Chars, out base64Chars) == false)
                {
                    return false;
                }

                base64 += base64Chars;
            }

            // because the TryMakeBase64Chars method matches tokens, each Text token could make
            // up several Base64 encoded "bytes" so we ensure that we have a length divisible by 4
            return base64 != null && base64.Length % 4 == 0;
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
    }
}
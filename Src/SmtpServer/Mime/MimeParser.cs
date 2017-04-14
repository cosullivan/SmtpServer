using System;
using SmtpServer.Text;

namespace SmtpServer.Mime
{
    public sealed class MimeVersion
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="number">The mime version.</param>
        public MimeVersion(decimal number)
        {
            Number = number;
        }

        /// <summary>
        /// The version number.
        /// </summary>
        public decimal Number { get; }
    }

    public abstract class MimeEntity
    {
        
    }

    public sealed class MimeHeader
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="name">The name of the MIME header.</param>
        /// <param name="value">The value for the MIME header.</param>
        public MimeHeader(string name, string value)
        {
            Name = name;
            Value = value;
        }

        /// <summary>
        /// The name of the header.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// The value for the header.
        /// </summary>
        public string Value { get; set; }
    }

    // https://tools.ietf.org/html/rfc822#section-3.2
    // https://tools.ietf.org/html/rfc2045
    // http://docs.roguewave.com/sourcepro/11.1/html/protocolsug/10-1.html
    public sealed class MimeParser : TokenParser
    {
        static readonly Token SpaceToken = new Token(TokenKind.Space, ' ');
        static readonly Token ColonToken = new Token(TokenKind.Punctuation, ':');
        static readonly Token DecimalPointToken = new Token(TokenKind.Punctuation, '.');

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="enumerator">The token enumerator to handle the incoming tokens.</param>
        public MimeParser(TokenEnumerator enumerator) : base(enumerator) { }

        /// <summary>
        /// Attempt to make a MIME version header.
        /// </summary>
        /// <param name="version">The MIME version header that was made.</param>
        /// <returns>true if the MIME version header could be made, false if not.</returns>
        public bool TryMakeMimeVersion(out MimeVersion version)
        {
            version = null;

            if (TryMakeFieldName(out string name) == false || name.CaseInsensitiveEquals("MIME-Version") == false)
            {
                return false;
            }

            Enumerator.TakeWhile(TokenKind.Space);

            if (Enumerator.Take() != ColonToken)
            {
                return false;
            }

            Enumerator.TakeWhile(TokenKind.Space);

            if (TryMakeDecimal(out decimal number) == false)
            {
                return false;
            }
            version = new MimeVersion(number);

            return TryMakeEnd();
        }

        /// <summary>
        /// Attempt to make the end of the line.
        /// </summary>
        /// <returns>true if the end of line could be made, false if not.</returns>
        bool TryMakeEnd()
        {
            Enumerator.TakeWhile(TokenKind.Space);

            return Enumerator.Peek() == Token.None;
        }

        /// <summary>
        /// Attempt to make a decimal number.
        /// </summary>
        /// <param name="number">The decimal number that was made.</param>
        /// <returns>true if the decimal number was made, false if not.</returns>
        bool TryMakeDecimal(out decimal number)
        {
            number = default(decimal);

            if (Enumerator.Peek().Kind != TokenKind.Number)
            {
                return false;
            }
            var scale = Enumerator.Take().Text;

            if (Enumerator.Take() != DecimalPointToken)
            {
                return false;
            }

            if (Enumerator.Peek().Kind != TokenKind.Number)
            {
                return false;
            }
            
            number = Decimal.Parse($"{scale}.{Enumerator.Take().Text}");
            return true;
        }

        //public bool TryMakeField(out MimeHeader header)
        //{
        //    header = null;

        //    string name;
        //    if (TryMake(TryMakeFieldName, out name) == false)
        //    {
        //        return false;
        //    }

        //    Enumerator.TakeWhile(TokenKind.Space);

        //    if (Enumerator.Take() != ColonToken)
        //    {
        //        return false;
        //    }

        //    Enumerator.TakeWhile(TokenKind.Space);

        //    string body;
        //    if (TryMake(TryMakeFieldBody, out body) == false)
        //    {
        //        return false;
        //    }

        //    header = new MimeHeader(name, body);
        //    return true;
        //}

        /// <summary>
        /// Attempt to make a MIME field name.
        /// </summary>
        /// <param name="name">The name of the field that was made.</param>
        /// <returns>true if a field name could be made, false if not.</returns>
        /// <remarks><![CDATA[1*<any CHAR, excluding CTLs, SPACE, and ":">]]></remarks>
        public bool TryMakeFieldName(out string name)
        {
            name = null;

            var token = Enumerator.Peek();
            while (token != Token.None && token != SpaceToken && token != ColonToken)
            {
                token = Enumerator.Take();
                switch (token.Kind)
                {
                    case TokenKind.Text:
                    case TokenKind.Number:
                        break;

                    case TokenKind.Space:
                    case TokenKind.Punctuation:
                        if (token.Text[0] <= 31)
                        {
                            return false;
                        }
                        break;

                    default:
                        return false;
                }

                name += token.Text;
                token = Enumerator.Peek();
            }

            return name != null;
        }

        //public bool TryMakeFieldBody(out string body)
        //{
        //    body = null;
        //    return true;
        //}

        //public bool TryMakeFieldBodyContents(out string name)
        //{
        //    name = null;
        //    return true;
        //}
    }
}
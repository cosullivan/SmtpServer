using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using SmtpServer.Content;
using SmtpServer.Text;

namespace SmtpServer.Mime
{
    public sealed class MimeVersion : IMimeHeader
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
        /// The name of the header.
        /// </summary>
        public string Name => "MIME-Version";

        /// <summary>
        /// The version number.
        /// </summary>
        public decimal Number { get; }
    }

    public sealed class ContentType : IMimeHeader
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="type">The media type.</param>
        /// <param name="subtype">The subtype.</param>
        /// <param name="parameters">The list of parameters.</param>
        public ContentType(string type, string subtype, IReadOnlyDictionary<string, string> parameters)
        {
            Type = type;
            SubType = subtype;
            Parameters = parameters;
        }

        /// <summary>
        /// The name of the header.
        /// </summary>
        public string Name => "Content-Type";

        /// <summary>
        /// The media type.
        /// </summary>
        public string Type { get; }

        /// <summary>
        /// The subtype.
        /// </summary>
        public string SubType { get; }

        /// <summary>
        /// The list of parameters.
        /// </summary>
        public IReadOnlyDictionary<string, string> Parameters { get; }
    }

    public abstract class MimeEntity
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="contentType">The content type of the entity.</param>
        protected MimeEntity(ContentType contentType)
        {
            ContentType = contentType;
        }

        /// <summary>
        /// The content type.
        /// </summary>
        public ContentType ContentType { get; }
    }

    public sealed class MimeMessage : IMimeMessage
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="version">The MIME version.</param>
        /// <param name="body">The body of the message.</param>
        public MimeMessage(MimeVersion version, MimeEntity body)
        {
            Version = version;
            Body = body;
        }

        /// <summary>
        /// The message type.
        /// </summary>
        MessageType IMessage.Type
        {
            get { return MessageType.Mime; }
        }

        /// <summary>
        /// The MIME version.
        /// </summary>
        public MimeVersion Version { get; }

        /// <summary>
        /// The message body.
        /// </summary>
        public MimeEntity Body { get; }
    }

    public interface IMimeHeader
    {
        /// <summary>
        /// The name of the header.
        /// </summary>
        string Name { get; }
    }

    public sealed class MimeHeader : IMimeHeader
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
        public string Value { get; }
    }

    public sealed class MessageParser : TokenParser
    {
        static readonly Token SpaceToken = new Token(TokenKind.Space, ' ');
        static readonly Token ColonToken = new Token(TokenKind.Punctuation, ':');

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="enumerator">The token enumerator to handle the incoming tokens.</param>
        public MessageParser(TokenEnumerator enumerator) : base(enumerator) { }

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

        /// <summary>
        /// Attempt to make the end of the line.
        /// </summary>
        /// <returns>true if the end of line could be made, false if not.</returns>
        bool TryMakeEnd()
        {
            Enumerator.TakeWhile(TokenKind.Space);

            return Enumerator.Peek() == Token.None;
        }
    }

    // https://tools.ietf.org/html/rfc822#section-3.2
    // https://tools.ietf.org/html/rfc2045
    // http://docs.roguewave.com/sourcepro/11.1/html/protocolsug/10-1.html
    public sealed class MimeParser : TokenParser
    {
        static readonly Token SpaceToken = new Token(TokenKind.Space, ' ');
        static readonly Token ColonToken = new Token(TokenKind.Punctuation, ':');
        static readonly Token DecimalPointToken = new Token(TokenKind.Punctuation, '.');
        static readonly Token EqualsToken = new Token(TokenKind.Symbol, '=');
        static readonly Token SemiColonToken = new Token(TokenKind.Punctuation, ';');
        static readonly Token[] DescreteTypeTokens = 
        {
            new Token(TokenKind.Text, "text"),
            new Token(TokenKind.Text, "image"),
            new Token(TokenKind.Text, "audio"),
            new Token(TokenKind.Text, "video"),
            new Token(TokenKind.Text, "application")
        };
        static readonly Token[] CompositeTypeTokens =
        {
            new Token(TokenKind.Text, "message"),
            new Token(TokenKind.Text, "multipart"),
        };

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
        /// Attempt to make a content type.
        /// </summary>
        /// <param name="contentType">The content type that was made.</param>
        /// <returns>true if a content type could be made, false if not.</returns>
        /// <remarks><![CDATA["Content-Type" ":" type "/" subtype]]></remarks>
        public bool TryMakeContentType(out ContentType contentType)
        {
            contentType = null;

            if (TryMakeFieldName(out string name) == false || name.CaseInsensitiveEquals("Content-Type") == false)
            {
                return false;
            }

            if (TryMakeMediaType(out string mediaType) == false)
            {
                return false;
            }

            if (TryMakeSubType(out string subType) == false)
            {
                return false;
            }

            TryMake(TryMakeParameterList, out Dictionary<string, string> parameters);

            contentType = new ContentType(mediaType, subType, parameters ?? new Dictionary<string, string>());

            return TryMakeEnd();
        }

        /// <summary>
        /// Attempt to make a content type media type.
        /// </summary>
        /// <param name="type">The type that was made.</param>
        /// <returns>true if a media type could be made, false if not.</returns>
        bool TryMakeMediaType(out string type)
        {
            return TryMake(TryMakeDescreteType, out type) || TryMake(TryMakeCompositeType, out type);
        }

        /// <summary>
        /// Attempt to make a content type subtype.
        /// </summary>
        /// <param name="type">The type that was made.</param>
        /// <returns>true if a subtype could be made, false if not.</returns>
        bool TryMakeSubType(out string type)
        {
            return TryMake(TryMakeExtensionToken, out type) || TryMake(TryMakeIanaToken, out type);
        }

        /// <summary>
        /// Attempt to make a descrete type.
        /// </summary>
        /// <param name="type">The descrete type that was made.</param>
        /// <returns>true if a descrete type could be made, false if not.</returns>
        bool TryMakeDescreteType(out string type)
        {
            if (DescreteTypeTokens.Contains(Enumerator.Peek()))
            {
                type = Enumerator.Take().Text;
                return true;
            }

            return TryMakeExtensionToken(out type);
        }

        /// <summary>
        /// Attempt to make a composite type.
        /// </summary>
        /// <param name="type">The composite type that was made.</param>
        /// <returns>true if a composite type could be made, false if not.</returns>
        bool TryMakeCompositeType(out string type)
        {
            if (CompositeTypeTokens.Contains(Enumerator.Peek()))
            {
                type = Enumerator.Take().Text;
                return true;
            }

            return TryMakeExtensionToken(out type);
        }

        /// <summary>
        /// Attempt to make an extension token.
        /// </summary>
        /// <param name="token">The extension token that was made.</param>
        /// <returns>true if an extension token could be made, false if not.</returns>
        bool TryMakeExtensionToken(out string token)
        {
            return TryMake(TryMakeIetfToken, out token) || TryMake(TryMakeXToken, out token);
        }

        bool TryMakeIetfToken(out string token)
        {
            token = null;

            return false;
        }

        bool TryMakeXToken(out string token)
        {
            token = null;

            return false;
        }

        bool TryMakeIanaToken(out string token)
        {
            token = null;

            return false;
        }

        /// <summary>
        /// Attempt to make a parameter list.
        /// </summary>
        /// <param name="parameters">The parameter list that was made.</param>
        /// <returns>true if the parameter list coud be made, false if not.</returns>
        bool TryMakeParameterList(out Dictionary<string, string> parameters)
        {
            parameters = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            Enumerator.TakeWhile(TokenKind.Space);
            while (Enumerator.Peek() == SemiColonToken)
            {
                Enumerator.Take();

                if (TryMakeParameterAttribute(out string attribute) == false)
                {
                    return false;
                }

                Enumerator.TakeWhile(TokenKind.Space);

                if (Enumerator.Take() != EqualsToken)
                {
                    return false;
                }

                Enumerator.TakeWhile(TokenKind.Space);

                if (TryMakeParameterValue(out string value) == false)
                {
                    return false;
                }

                parameters[attribute] = value;

                Enumerator.TakeWhile(TokenKind.Space);
            }

            return true;
        }

        /// <summary>
        /// Attempt to make a content type parameter.
        /// </summary>
        /// <param name="attribute">The attribute for the parameter.</param>
        /// <param name="value">The value for the parameter.</param>
        /// <returns>true if the parameter could be made, false if not.</returns>
        bool TryMakeParameter(out string attribute, out string value)
        {
            value = null;

            if (TryMakeParameterAttribute(out attribute) == false)
            {
                return false;
            }

            Enumerator.TakeWhile(TokenKind.Space);

            if (Enumerator.Take() != EqualsToken)
            {
                return false;
            }

            Enumerator.TakeWhile(TokenKind.Space);

            return TryMakeParameterValue(out value);
        }

        bool TryMakeParameterAttribute(out string attribute)
        {
            attribute = null;

            return false;
        }

        bool TryMakeParameterValue(out string value)
        {
            value = null;

            return false;
        }

        bool TryMakeToken(out string token)
        {
            token = null;

            return false;
        }

        bool TryMakeQuotedString(out string quotedString)
        {
            quotedString = null;

            return false;
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
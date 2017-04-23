using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SmtpServer.Content;
using SmtpServer.Text;

namespace SmtpServer.Mime
{
    public sealed class MimeVersion : IMimeHeader
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="major">The major version number.</param>
        /// <param name="minor">The minor version number.</param>
        public MimeVersion(int major, int minor)
        {
            Major = major;
            Minor = minor;
        }

        /// <summary>
        /// The name of the header.
        /// </summary>
        public string Name => "MIME-Version";

        /// <summary>
        /// The major version number.
        /// </summary>
        public int Major { get; }

        /// <summary>
        /// The minor version number.
        /// </summary>
        public int Minor { get; }
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
        MessageType IMessage.Type => MessageType.Mime;

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
        /// <param name="tokens">The list of tokens that make up the field body.</param>
        public MimeHeader(string name, IReadOnlyList<Token> tokens)
        {
            Name = name;
            Tokens = tokens;
        }

        /// <summary>
        /// Returns a string representation of the header.
        /// </summary>
        /// <returns>The string representation of the header.</returns>
        public override string ToString()
        {
            return $"{Name}:{Value}";
        }

        /// <summary>
        /// The name of the header.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// The list of tokens that make up the field body.
        /// </summary>
        public IReadOnlyList<Token> Tokens { get; }

        /// <summary>
        /// The value for the header.
        /// </summary>
        public string Value
        {
            get { return String.Concat(Tokens.Select(token => token.Text)); }
        }
    }

    //public struct MakeResult<TValue>
    //{
    //    public static readonly MakeResult<TValue> Failed = new MakeResult<TValue>(false);

    //    /// <summary>
    //    /// Constructor.
    //    /// </summary>
    //    /// <param name="result">The result that indicates whether a match was a success or not.</param>
    //    public MakeResult(bool result) : this(result, default(TValue)) { }

    //    /// <summary>
    //    /// Constructor.
    //    /// </summary>
    //    /// <param name="result">The result that indicates whether a match was a success or not.</param>
    //    /// <param name="value">The value of the match.</param>
    //    public MakeResult(bool result, TValue value)
    //    {
    //        Result = result;
    //        Value = value;
    //    }

    //    /// <summary>
    //    /// Indicates whether or not a make operation was a success.
    //    /// </summary>
    //    public bool Result { get; }

    //    /// <summary>
    //    /// The value that was made.
    //    /// </summary>
    //    public TValue Value { get; }
    //}

    // https://tools.ietf.org/html/rfc822#section-3.2
    // https://tools.ietf.org/html/rfc2045
    // http://docs.roguewave.com/sourcepro/11.1/html/protocolsug/10-1.html
    public sealed class MimeParser : TokenParser
    {
        static readonly Token SpaceToken = new Token(TokenKind.Space, ' ');
        static readonly Token CrToken = new Token(TokenKind.Space, (char)13);
        static readonly Token LfToken = new Token(TokenKind.Space, (char)10);
        static readonly Token HtabToken = new Token(TokenKind.Space, (char)9);
        static readonly Token QuoteToken = new Token(TokenKind.Punctuation, (char)34);
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
        public MimeParser(ITokenEnumerator enumerator) : base(enumerator) { }

        /// <summary>
        /// Attempt to make a MIME version header.
        /// </summary>
        /// <returns>The result of the operation that indicates the state and optionally value.</returns>
        public bool TryMakeMimeVersion(out MimeVersion version)
        {
            version = null;

            if (TryMakeFieldName(out string name) == false || name.CaseInsensitiveEquals("MIME-Version") == false)
            {
                return false;
            }

            Enumerator.Skip(TokenKind.Space);

            var major = Enumerator.Take();
            if (major.Kind != TokenKind.Number)
            {
                return false;
            }

            if (Enumerator.Take() != DecimalPointToken)
            {
                return false;
            }

            var minor = Enumerator.Take();
            if (minor.Kind != TokenKind.Number)
            {
                return false;
            }

            version = new MimeVersion(Int32.Parse(major.Text), Int32.Parse(minor.Text));

            return TryMakeEnd();
        }

        ///// <summary>
        ///// Attempt to make a content type.
        ///// </summary>
        ///// <param name="contentType">The content type that was made.</param>
        ///// <returns>true if a content type could be made, false if not.</returns>
        ///// <remarks><![CDATA["Content-Type" ":" type "/" subtype]]></remarks>
        //public bool TryMakeContentType(out ContentType contentType)
        //{
        //    contentType = null;

        //    if (TryMakeFieldName(out string name) == false || name.CaseInsensitiveEquals("Content-Type") == false)
        //    {
        //        return false;
        //    }

        //    if (TryMakeMediaType(out string mediaType) == false)
        //    {
        //        return false;
        //    }

        //    if (TryMakeSubType(out string subType) == false)
        //    {
        //        return false;
        //    }

        //    TryMake(TryMakeParameterList, out Dictionary<string, string> parameters);

        //    contentType = new ContentType(mediaType, subType, parameters ?? new Dictionary<string, string>());

        //    return TryMakeEnd();
        //}

        ///// <summary>
        ///// Attempt to make a content type media type.
        ///// </summary>
        ///// <param name="type">The type that was made.</param>
        ///// <returns>true if a media type could be made, false if not.</returns>
        //bool TryMakeMediaType(out string type)
        //{
        //    return TryMake(TryMakeDescreteType, out type) || TryMake(TryMakeCompositeType, out type);
        //}

        ///// <summary>
        ///// Attempt to make a content type subtype.
        ///// </summary>
        ///// <param name="type">The type that was made.</param>
        ///// <returns>true if a subtype could be made, false if not.</returns>
        //bool TryMakeSubType(out string type)
        //{
        //    return TryMake(TryMakeExtensionToken, out type) || TryMake(TryMakeIanaToken, out type);
        //}

        ///// <summary>
        ///// Attempt to make a descrete type.
        ///// </summary>
        ///// <param name="type">The descrete type that was made.</param>
        ///// <returns>true if a descrete type could be made, false if not.</returns>
        //bool TryMakeDescreteType(out string type)
        //{
        //    if (DescreteTypeTokens.Contains(Enumerator.Peek()))
        //    {
        //        type = Enumerator.Take().Text;
        //        return true;
        //    }

        //    return TryMakeExtensionToken(out type);
        //}

        ///// <summary>
        ///// Attempt to make a composite type.
        ///// </summary>
        ///// <param name="type">The composite type that was made.</param>
        ///// <returns>true if a composite type could be made, false if not.</returns>
        //bool TryMakeCompositeType(out string type)
        //{
        //    if (CompositeTypeTokens.Contains(Enumerator.Peek()))
        //    {
        //        type = Enumerator.Take().Text;
        //        return true;
        //    }

        //    return TryMakeExtensionToken(out type);
        //}

        ///// <summary>
        ///// Attempt to make an extension token.
        ///// </summary>
        ///// <param name="token">The extension token that was made.</param>
        ///// <returns>true if an extension token could be made, false if not.</returns>
        //bool TryMakeExtensionToken(out string token)
        //{
        //    return TryMake(TryMakeIetfToken, out token) || TryMake(TryMakeXToken, out token);
        //}

        //bool TryMakeIetfToken(out string token)
        //{
        //    token = null;

        //    return false;
        //}

        //bool TryMakeXToken(out string token)
        //{
        //    token = null;

        //    return false;
        //}

        //bool TryMakeIanaToken(out string token)
        //{
        //    token = null;

        //    return false;
        //}

        ///// <summary>
        ///// Attempt to make a parameter list.
        ///// </summary>
        ///// <param name="parameters">The parameter list that was made.</param>
        ///// <returns>true if the parameter list coud be made, false if not.</returns>
        //bool TryMakeParameterList(out Dictionary<string, string> parameters)
        //{
        //    parameters = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        //    Enumerator.TakeWhile(TokenKind.Space);
        //    while (Enumerator.Peek() == SemiColonToken)
        //    {
        //        Enumerator.Take();

        //        if (TryMakeParameterAttribute(out string attribute) == false)
        //        {
        //            return false;
        //        }

        //        Enumerator.TakeWhile(TokenKind.Space);

        //        if (Enumerator.Take() != EqualsToken)
        //        {
        //            return false;
        //        }

        //        Enumerator.TakeWhile(TokenKind.Space);

        //        if (TryMakeParameterValue(out string value) == false)
        //        {
        //            return false;
        //        }

        //        parameters[attribute] = value;

        //        Enumerator.TakeWhile(TokenKind.Space);
        //    }

        //    return true;
        //}

        ///// <summary>
        ///// Attempt to make a content type parameter.
        ///// </summary>
        ///// <param name="attribute">The attribute for the parameter.</param>
        ///// <param name="value">The value for the parameter.</param>
        ///// <returns>true if the parameter could be made, false if not.</returns>
        //bool TryMakeParameter(out string attribute, out string value)
        //{
        //    value = null;

        //    if (TryMakeParameterAttribute(out attribute) == false)
        //    {
        //        return false;
        //    }

        //    Enumerator.TakeWhile(TokenKind.Space);

        //    if (Enumerator.Take() != EqualsToken)
        //    {
        //        return false;
        //    }

        //    Enumerator.TakeWhile(TokenKind.Space);

        //    return TryMakeParameterValue(out value);
        //}

        //bool TryMakeParameterAttribute(out string attribute)
        //{
        //    attribute = null;

        //    return false;
        //}

        //bool TryMakeParameterValue(out string value)
        //{
        //    value = null;

        //    return false;
        //}

        //bool TryMakeToken(out string token)
        //{
        //    token = null;

        //    return false;
        //}

        //bool TryMakeQuotedString(out string quotedString)
        //{
        //    quotedString = null;

        //    return false;
        //}

        /// <summary>
        /// Attempt to make a mime header field list.
        /// </summary>
        /// <param name="mimeHeaders">The list of headers that was found.</param>
        /// <returns>true if a header field list could be made, false if not.</returns>
        internal bool TryMakeFieldList(out List<IMimeHeader> mimeHeaders)
        {
            mimeHeaders = new List<IMimeHeader>();

            while (Enumerator.Peek() != Token.None)
            {
                if (TryMakeField(out IMimeHeader mimeHeader) == false)
                {
                    return false;
                }

                mimeHeaders.Add(mimeHeader);
            }

            return true;
        }

        /// <summary>
        /// Attempt to make a field.
        /// </summary>
        /// <param name="mimeHeader">The mime header that was made.</param>
        /// <returns>true if a MIME header field could be made, false if not.</returns>
        internal bool TryMakeField(out IMimeHeader mimeHeader)
        {
            mimeHeader = null;

            if (TryMakeFieldName(out string name) == false)
            {
                return false;
            }

            if (TryMakeFieldBody(out List<Token> body) == false)
            {
                return false;
            }

            mimeHeader = new MimeHeader(name, body);

            return TryMakeEnd();
        }

        /// <summary>
        /// Attempt to make a MIME header field body.
        /// </summary>
        /// <param name="body">The list of tokens that were matched for the body.</param>
        /// <returns>true if a MIME header field body could be made, false if not.</returns>
        internal bool TryMakeFieldBody(out List<Token> body)
        {
            if (TryMakeFieldBodyContents(out body) == false)
            {
                return false;
            }

            while (TryMake(TryMakeCrlfLwsp, out Token token1, out Token token2))
            {
                body.AddRange(new[] { token1, token2 });

                if (TryMakeFieldBodyContents(out List<Token> bodyContents) == false)
                {
                    return false;
                }

                body.AddRange(bodyContents);
            }

            return true;
        }

        /// <summary>
        /// Attempt to make a CRLF LWSP component.
        /// </summary>
        /// <param name="token1">The CRLF token that was made.</param>
        /// <param name="token2">The LWSP token that was made.</param>
        /// <returns>true if the CRLF LWSP token combination could be made.</returns>
        /// <remarks>This represents the long line folding tokens.</remarks>
        internal bool TryMakeCrlfLwsp(out Token token1, out Token token2)
        {
            token1 = Enumerator.Take();

            return TryMakeLwsp(out token2) && token1 == Token.NewLine;
        }
        
        /// <summary>
        /// Try make a LWSP token.
        /// </summary>
        /// <param name="token">The token that was made.</param>
        /// <returns>true if the LWSP token could be made, false if not.</returns>
        internal bool TryMakeLwsp(out Token token)
        {
            token = Enumerator.Take();

            return token == SpaceToken || token == HtabToken;
        }

        /// <summary>
        /// Attempt to make the field body contents.
        /// </summary>
        /// <param name="bodyContents">The list of tokens that were made for the contents of the field body.</param>
        /// <returns>true if the field body contents could be made, false if not.</returns>
        internal bool TryMakeFieldBodyContents(out List<Token> bodyContents)
        {
            bodyContents = new List<Token>();

            while (Enumerator.Peek() != Token.NewLine)
            {
                bodyContents.Add(Enumerator.Take());
            }

            return bodyContents.Count > 0;
        }

        /// <summary>
        /// Attempt to make the end of the line.
        /// </summary>
        /// <returns>true if the end of line could be made, false if not.</returns>
        internal bool TryMakeEnd()
        {
            Enumerator.Skip(TokenKind.Space);

            return Enumerator.Take() == Token.NewLine;
        }

        /// <summary>
        /// Attempt to make a MIME field name.
        /// </summary>
        /// <param name="name">The name of the field that was made.</param>
        /// <returns>true if a field name could be made, false if not.</returns>
        /// <remarks><![CDATA[1*<any CHAR, excluding CTLs, SPACE, and ":">]]></remarks>
        internal bool TryMakeFieldName(out string name)
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

            return name != null && Enumerator.Take() == ColonToken;
        }
    }
}
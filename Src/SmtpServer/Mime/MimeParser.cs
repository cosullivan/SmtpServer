using System;
using System.Collections.Generic;
using System.IO;
using SmtpServer.Text;
using System.Linq;
using System.Xml.Schema;
using SmtpServer.IO;

namespace SmtpServer.Mime
{
    public sealed class MimeParser : TokenParser
    {
        #region Tokens

        static class Tokens
        {
            // ReSharper disable InconsistentNaming
            internal static readonly Token CR = Token.Create(13);
            internal static readonly Token LF = Token.Create(10);
            internal static readonly Token Space = Token.Create(' ');
            internal static readonly Token HTAB = Token.Create(9);
            internal static readonly Token Quote = Token.Create(34);
            internal static readonly Token Colon = Token.Create(':');
            internal static readonly Token DecimalPoint = Token.Create('.');
            internal static readonly Token Equal = Token.Create('=');
            internal static readonly Token Dash = Token.Create('-');
            internal static readonly Token SemiColon = Token.Create(';');
            internal static readonly Token ForwardSlash = Token.Create('/');
            internal static readonly Token BackSlash = Token.Create('\\');
            internal static readonly Token NewLine = Token.Create(TokenKind.NewLine, new byte[] { 13, 10 });
            internal static readonly Token[] TSpecials = 
            {
                Token.Create('('),
                Token.Create(')'),
                Token.Create('<'),
                Token.Create('>'),
                Token.Create('@'),
                Token.Create(','),
                Token.Create(';'),
                Token.Create(':'),
                Token.Create('\\'),
                Token.Create('"'),
                Token.Create('/'),
                Token.Create('['),
                Token.Create(']'),
                Token.Create('?'),
                Token.Create('=')
            };
            internal static readonly Token[] CTL = 
            {
                Token.Create(0),
                Token.Create(1),
                Token.Create(2),
                Token.Create(3),
                Token.Create(4),
                Token.Create(5),
                Token.Create(6),
                Token.Create(7),
                Token.Create(8),
                Token.Create(9),
                Token.Create(10),
                Token.Create(11),
                Token.Create(12),
                Token.Create(13),
                Token.Create(14),
                Token.Create(15),
                Token.Create(16),
                Token.Create(17),
                Token.Create(18),
                Token.Create(19),
                Token.Create(20),
                Token.Create(21),
                Token.Create(22),
                Token.Create(23),
                Token.Create(24),
                Token.Create(25),
                Token.Create(26),
                Token.Create(27),
                Token.Create(28),
                Token.Create(29),
                Token.Create(30),
                Token.Create(31),
                Token.Create(127),
            };
            internal static readonly Token[] DescreteTypes =
            {
                Token.Create("text"),
                Token.Create("image"),
                Token.Create("audio"),
                Token.Create("video"),
                Token.Create("application")
            };
            internal static readonly Token[] CompositeTypes =
            {
                Token.Create("message"),
                Token.Create("multipart"),
            };
            internal static readonly Token SevenBit = Token.Create("7bit");
            internal static readonly Token EightBit = Token.Create("8bit");
            internal static readonly Token Binary = Token.Create("binary");
            internal static readonly Token Base64 = Token.Create("base64");
            // ReSharper restore InconsistentNaming
        }

        #endregion

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="enumerator">The token enumerator to handle the incoming tokens.</param>
        public MimeParser(ITokenEnumerator enumerator) : base(enumerator) { }

        /// <summary>
        /// Attempt to make a MIME document.
        /// </summary>
        /// <param name="document">The document that was made.</param>
        /// <returns>true if the document could be made, false if not.</returns>
        public bool TryMakeDocument(out MimeDocument document)
        {
            document = null;

            if (TryMakeFieldList(out List<IMimeHeader> headers) == false)
            {
                return false;
            }

            var version = headers.OfType<MimeVersion>().SingleOrDefault();
            if (version == null || (version.Major != 1 && version.Minor != 0))
            {
                return false;
            }

            if (TryMakeEntity(headers, out MimeEntity entity) == false)
            {
                return false;
            }

            document = new MimeDocument(version, entity);
            return true;
        }

        /// <summary>
        /// Attempt to make a MIME entity.
        /// </summary>
        /// <param name="entity">The entity that was made.</param>
        /// <returns>true if the entity could be made, false if not.</returns>
        public bool TryMakeEntity(out MimeEntity entity)
        {
            entity = null;

            if (TryMakeFieldList(out List<IMimeHeader> headers) == false)
            {
                // TODO: are the headers optional?
                return false;
            }

            return TryMakeEntity(headers, out entity);
        }

        /// <summary>
        /// Attempt to make a MIME entity.
        /// </summary>
        /// <param name="headers">The list of header fields that were made.</param>
        /// <param name="entity">The entity that was made.</param>
        /// <returns>true if the entity could be made, false if not.</returns>
        public bool TryMakeEntity(List<IMimeHeader> headers, out MimeEntity entity)
        {
            entity = null;

            var type = headers.OfType<ContentType>().SingleOrDefault() ?? ContentType.TextPlain;

            if (TryMakeContent(type, out Stream content) == false)
            {
                return false;
            }

            entity = new MimePart(headers, content);
            return true;
        }

        /// <summary>
        /// Attempt to make the MIME content.
        /// </summary>
        /// <param name="type">The content type to make.</param>
        /// <param name="stream">The stream that represents the content.</param>
        /// <returns>true if the content was made, false if not.</returns>
        internal bool TryMakeContent(ContentType type, out Stream stream)
        {
            if (type.MediaType.ToLower() == "text")
            {
                return TryMakeTextContent(out stream);
            }

            stream = null;
            return false;
        }

        /// <summary>
        /// Attempt to make the MIME content text.
        /// </summary>
        /// <param name="stream">The stream that represents the content.</param>
        /// <returns>true if the content was made, false if not.</returns>
        internal bool TryMakeTextContent(out Stream stream)
        {
            var offset = Enumerator.Position;

            Enumerator.Skip(token => token != Token.None);

            stream = CreateTokenStream(offset, Enumerator.Position - offset);
            return true;
        }

        /// <summary>
        /// Attempt to make tokens until a given sequence has been reached.
        /// </summary>
        /// <param name="sequence">The sequence to make.</param>
        /// <param name="offset">The offset into the token list that the match started from.</param>
        /// <param name="count">The number of tokens that were made.</param>
        /// <returns>true if the sequence was found, false if not.</returns>
        bool TryMakeUntil(Token[] sequence, out int offset, out int count)
        {
            offset = Enumerator.Position;

            var found = 0;
            while (Enumerator.Peek() != Token.None && found < sequence.Length)
            {
                var current = Enumerator.Take();

                found = current == sequence[found]
                    ? found + 1
                    : current == sequence[0] ? 1 : 0;
            }

            count = Enumerator.Position - offset;
            return found == sequence.Length;
        }

        /// <summary>
        /// Attempt to make a mime header field list.
        /// </summary>
        /// <param name="mimeHeaders">The list of headers that was found.</param>
        /// <returns>true if a header field list could be made, false if not.</returns>
        internal bool TryMakeFieldList(out List<IMimeHeader> mimeHeaders)
        {
            mimeHeaders = new List<IMimeHeader>();

            if (Enumerator.Peek() == Token.None)
            {
                return false;
            }

            while (TryMake(TryMakeField, out IMimeHeader mimeHeader))
            {
                mimeHeaders.Add(mimeHeader);
            }

            return TryMakeFieldEnd();
        }

        /// <summary>
        /// Attempt to make the end of the headers.
        /// </summary>
        /// <returns>true if the end of line could be made, false if not.</returns>
        internal bool TryMakeEnd()
        {
            Enumerator.Skip(TokenKind.Space);

            return Enumerator.Take() == Token.None;
        }

        /// <summary>
        /// Attempt to make a field.
        /// </summary>
        /// <param name="mimeHeader">The mime header that was made.</param>
        /// <returns>true if a MIME header field could be made, false if not.</returns>
        internal bool TryMakeField(out IMimeHeader mimeHeader)
        {
            if (TryMake(TryMakeKnownField, out mimeHeader) || TryMakeUnknownField(out mimeHeader))
            {
                return TryMakeFieldEnd();
            }

            return false;
        }

        /// <summary>
        /// Attempt to make a field.
        /// </summary>
        /// <param name="mimeHeader">The mime header that was made.</param>
        /// <returns>true if a MIME header field could be made, false if not.</returns>
        internal bool TryMakeKnownField(out IMimeHeader mimeHeader)
        {
            return TryMake(TryMakeMimeVersion, out mimeHeader)
                || TryMake(TryMakeContentType, out mimeHeader)
                || TryMake(TryMakeContentTransferEncoding, out mimeHeader);
        }
        
        /// <summary>
        /// Attempt to make a field.
        /// </summary>
        /// <param name="mimeHeader">The mime header that was made.</param>
        /// <returns>true if a MIME header field could be made, false if not.</returns>
        internal bool TryMakeUnknownField(out IMimeHeader mimeHeader)
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

            return true;
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

            return TryMakeLwsp(out token2) && token1.Kind == TokenKind.NewLine;
        }
        
        /// <summary>
        /// Try make a LWSP token.
        /// </summary>
        /// <param name="token">The token that was made.</param>
        /// <returns>true if the LWSP token could be made, false if not.</returns>
        internal bool TryMakeLwsp(out Token token)
        {
            token = Enumerator.Take();

            return token == Tokens.Space || token == Tokens.HTAB;
        }

        /// <summary>
        /// Attempt to make the field body contents.
        /// </summary>
        /// <param name="bodyContents">The list of tokens that were made for the contents of the field body.</param>
        /// <returns>true if the field body contents could be made, false if not.</returns>
        internal bool TryMakeFieldBodyContents(out List<Token> bodyContents)
        {
            bodyContents = new List<Token>();

            while (Enumerator.Peek().Kind != TokenKind.NewLine && Enumerator.Peek() != Token.None)
            {
                bodyContents.Add(Enumerator.Take());
            }

            return bodyContents.Count > 0;
        }

        /// <summary>
        /// Attempt to make a MIME version header.
        /// </summary>
        /// <returns>The result of the operation that indicates the state and optionally value.</returns>
        internal bool TryMakeMimeVersion(out IMimeHeader version)
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

            if (Enumerator.Take() != Tokens.DecimalPoint)
            {
                return false;
            }

            var minor = Enumerator.Take();
            if (minor.Kind != TokenKind.Number)
            {
                return false;
            }

            version = new MimeVersion(Int32.Parse(major.Text()), Int32.Parse(minor.Text()));
            return true;
        }

        /// <summary>
        /// Attempt to make a content type.
        /// </summary>
        /// <param name="contentType">The content type that was made.</param>
        /// <returns>true if a content type could be made, false if not.</returns>
        /// <remarks><![CDATA["Content-Type" ":" type "/" subtype]]></remarks>
        internal bool TryMakeContentType(out IMimeHeader contentType)
        {
            contentType = null;

            if (TryMakeFieldName(out string name) == false || name.CaseInsensitiveEquals("Content-Type") == false)
            {
                return false;
            }

            Enumerator.Skip(TokenKind.Space);

            if (TryMakeMediaType(out string mediaType) == false)
            {
                return false;
            }

            if (Enumerator.Take() != Tokens.ForwardSlash)
            {
                return false;
            }

            if (TryMakeMediaSubType(out string mediaSubType) == false)
            {
                return false;
            }

            if (TryMake(TryMakeOptionalParameterList, out Dictionary<string, string> parameters) == false)
            {
                return false;
            }

            contentType = new ContentType(mediaType, mediaSubType, parameters ?? new Dictionary<string, string>());
            return true;
        }

        /// <summary>
        /// Attempt to make a content transfer encoding.
        /// </summary>
        /// <param name="contentTransferEncoding">The content transfer encoding that was made.</param>
        /// <returns>true if a content transfer encoding could be made, false if not.</returns>
        /// <remarks><![CDATA["Content-Transfer-Encoding" ":" mechanism]]></remarks>
        internal bool TryMakeContentTransferEncoding(out IMimeHeader contentTransferEncoding)
        {
            contentTransferEncoding = null;

            if (TryMakeFieldName(out string name) == false || name.CaseInsensitiveEquals("Content-Transfer-Encoding") == false)
            {
                return false;
            }

            Enumerator.Skip(TokenKind.Space);

            if (TryMakeContentTransferEncodingMechanism(out string mechanism) == false)
            {
                return false;
            }

            if (ContentTransferEncoding.KnownEncodings.TryGetValue(mechanism, out ContentTransferEncoding knownEncoding))
            {
                contentTransferEncoding = knownEncoding;
                return true;
            }

            contentTransferEncoding = new ContentTransferEncoding(mechanism);
            return true;
        }

        /// <summary>
        /// Attempt to make a content transfer encoding mechanism.
        /// </summary>
        /// <param name="mechanism">The content transfer encoding mechanism that was made.</param>
        /// <returns>true if a content transfer encoding mechanism could be made, false if not.</returns>
        /// <remarks><![CDATA["7bit" / "8bit" / "binary" / "quoted-printable" / "base64" / ietf-token / x-token]]></remarks>
        internal bool TryMakeContentTransferEncodingMechanism(out string mechanism)
        {
            return TryMake(TryMakeKnownContentTransferEncodingMechanism, out mechanism) 
                || TryMake(TryMakeIetfToken, out mechanism)
                || TryMake(TryMakeXToken, out mechanism);
        }

        /// <summary>
        /// Attempt to make a known content transfer encoding mechanism.
        /// </summary>
        /// <param name="mechanism">The content transfer encoding mechanism that was made.</param>
        /// <returns>true if a content transfer encoding mechanism could be made, false if not.</returns>
        /// <remarks><![CDATA["7bit" / "8bit" / "binary" / "quoted-printable" / "base64"]]></remarks>
        internal bool TryMakeKnownContentTransferEncodingMechanism(out string mechanism)
        {
            return TryMake(TryMake7BitContentTransferEncodingMechanism, out mechanism)
                || TryMake(TryMake8BitContentTransferEncodingMechanism, out mechanism)
                || TryMake(TryMakeBinaryContentTransferEncodingMechanism, out mechanism)
                || TryMake(TryMakeBase64ContentTransferEncodingMechanism, out mechanism)
                || TryMake(TryMakeQuotedPrintableContentTransferEncodingMechanism, out mechanism);
        }

        /// <summary>
        /// Attempt to make a known 7Bit content transfer encoding mechanism.
        /// </summary>
        /// <param name="mechanism">The content transfer encoding mechanism that was made.</param>
        /// <returns>true if a content transfer encoding mechanism could be made, false if not.</returns>
        /// <remarks><![CDATA["7bit"]]></remarks>
        internal bool TryMake7BitContentTransferEncodingMechanism(out string mechanism)
        {
            mechanism = "7bit";

            return Enumerator.Take() == Tokens.SevenBit;
        }

        /// <summary>
        /// Attempt to make a known 8bit content transfer encoding mechanism.
        /// </summary>
        /// <param name="mechanism">The content transfer encoding mechanism that was made.</param>
        /// <returns>true if a content transfer encoding mechanism could be made, false if not.</returns>
        /// <remarks><![CDATA["8bit"]]></remarks>
        internal bool TryMake8BitContentTransferEncodingMechanism(out string mechanism)
        {
            mechanism = "8bit";

            return Enumerator.Take() == Tokens.EightBit;
        }

        /// <summary>
        /// Attempt to make a known binary content transfer encoding mechanism.
        /// </summary>
        /// <param name="mechanism">The content transfer encoding mechanism that was made.</param>
        /// <returns>true if a content transfer encoding mechanism could be made, false if not.</returns>
        /// <remarks><![CDATA["binary"]]></remarks>
        internal bool TryMakeBinaryContentTransferEncodingMechanism(out string mechanism)
        {
            mechanism = "binary";

            return Enumerator.Take() == Tokens.Binary;
        }

        /// <summary>
        /// Attempt to make a known Base64 content transfer encoding mechanism.
        /// </summary>
        /// <param name="mechanism">The content transfer encoding mechanism that was made.</param>
        /// <returns>true if a content transfer encoding mechanism could be made, false if not.</returns>
        /// <remarks><![CDATA["base64"]]></remarks>
        internal bool TryMakeBase64ContentTransferEncodingMechanism(out string mechanism)
        {
            mechanism = "base64";

            return Enumerator.Take() == Tokens.Base64;
        }

        /// <summary>
        /// Attempt to make a known Quoted-Printable content transfer encoding mechanism.
        /// </summary>
        /// <param name="mechanism">The content transfer encoding mechanism that was made.</param>
        /// <returns>true if a content transfer encoding mechanism could be made, false if not.</returns>
        /// <remarks><![CDATA["quoted-printable"]]></remarks>
        internal bool TryMakeQuotedPrintableContentTransferEncodingMechanism(out string mechanism)
        {
            mechanism = "quoted-printable";

            return TryTakeTokens(Token.Create(TokenKind.Text, "quoted"), Tokens.Dash, Token.Create(TokenKind.Text, "printable"));
        }

        /// <summary>
        /// Attempt to make a media type.
        /// </summary>
        /// <param name="type">The type that was made.</param>
        /// <returns>true if a media type could be made, false if not.</returns>
        /// <remarks><![CDATA[discrete-type / composite-type]]></remarks>
        bool TryMakeMediaType(out string type)
        {
            return TryMake(TryMakeDescreteType, out type) || TryMake(TryMakeCompositeType, out type);
        }

        /// <summary>
        /// Attempt to make a media subtype.
        /// </summary>
        /// <param name="type">The type that was made.</param>
        /// <returns>true if a subtype could be made, false if not.</returns>
        /// <remarks><![CDATA[extension-token / iana-token]]></remarks>
        bool TryMakeMediaSubType(out string type)
        {
            return TryMake(TryMakeExtensionToken, out type) || TryMake(TryMakeIanaToken, out type);
        }

        /// <summary>
        /// Attempt to make a descrete type.
        /// </summary>
        /// <param name="type">The descrete type that was made.</param>
        /// <returns>true if a descrete type could be made, false if not.</returns>
        /// <remarks><![CDATA["text" / "image" / "audio" / "video" / "application" / extension-token]]></remarks>
        bool TryMakeDescreteType(out string type)
        { 
            if (Tokens.DescreteTypes.Contains(Enumerator.Peek()))
            {
                type = Enumerator.Take().Text();
                return true;
            }

            return TryMakeExtensionToken(out type);
        }

        /// <summary>
        /// Attempt to make a composite type.
        /// </summary>
        /// <param name="type">The composite type that was made.</param>
        /// <returns>true if a composite type could be made, false if not.</returns>
        /// <remarks><![CDATA["message" / "multipart" / extension-token]]></remarks>
        bool TryMakeCompositeType(out string type)
        {
            if (Tokens.CompositeTypes.Contains(Enumerator.Peek()))
            {
                type = Enumerator.Take().Text();
                return true;
            }

            return TryMakeExtensionToken(out type);
        }

        /// <summary>
        /// Attempt to make an extension token.
        /// </summary>
        /// <param name="token">The extension token that was made.</param>
        /// <returns>true if an extension token could be made, false if not.</returns>
        /// <remarks><![CDATA[ietf-token / x-token]]></remarks>
        bool TryMakeExtensionToken(out string token)
        {
            return TryMake(TryMakeIetfToken, out token) || TryMake(TryMakeXToken, out token);
        }

        /// <summary>
        /// Attempt to make an IETF token.
        /// </summary>
        /// <param name="token">The IETF token that was matched.</param>
        /// <returns>true if an IETF token could be made, false if not.</returns>
        /// <remarks>An extension token defined by a standards-track RFC and registered with IANA.</remarks>
        bool TryMakeIetfToken(out string token)
        {
            token = null;

            return false;
        }

        /// <summary>
        /// Attempt to make an XToken.
        /// </summary>
        /// <param name="xtoken">The name of the x-token that was made.</param>
        /// <returns>true if an XToken could be made, false if not.</returns>
        /// <remarks>The two characters "X-" or "x-" followed, with no intervening white space, by any token.</remarks>
        bool TryMakeXToken(out string xtoken)
        {
            xtoken = Enumerator.Take().Text();

            if (xtoken == null || xtoken.CaseInsensitiveEquals("X") == false)
            {
                return false;
            }

            if (Enumerator.Take() != Tokens.Dash)
            {
                return false;
            }

            if (TryMakeToken(out string token) == false)
            {
                return false;
            }

            xtoken += "-" + token;
            return true;
        }

        /// <summary>
        /// Attempt to make an IANA token.
        /// </summary>
        /// <param name="token">The token that was made.</param>
        /// <returns>true if a token could be made, false if not.</returns>
        /// <remarks>A publicly-defined extension token. Tokens of this form must be registered with IANA as specified in RFC 2048</remarks>
        bool TryMakeIanaToken(out string token)
        {
            return TryMakeRegName(out token);
        }

        /// <summary>
        /// Attempt to make an IANA registered name.
        /// </summary>
        /// <param name="name">The registered name that was made.</param>
        /// <returns>true if a registered name could be made, false if not.</returns>
        /// <remarks>This comes from a stricter interperetation according to RFC4288.</remarks>
        /// <remarks><![CDATA[1*127reg-name-chars]]></remarks>
        bool TryMakeRegName(out string name)
        {
            name = null;

            var token = Enumerator.Peek();
            while (new[] { Token.None, Tokens.NewLine, Tokens.Space, Tokens.SemiColon }.Contains(token) == false)
            {
                if (TryMakeRegNameChars(out token) == false)
                {
                    return false;
                }

                name += token.Text();
                token = Enumerator.Peek();
            }

            return name?.Length <= 127;
        }

        /// <summary>
        /// Attempt to make an IANA registered name character subset.
        /// </summary>
        /// <param name="token">The registered name character subset that was made.</param>
        /// <returns>true if a registered name character subset could be made, false if not.</returns>
        /// <remarks><![CDATA[ALPHA / DIGIT / "!" / "#" / "$" / "&" / "." / "+" / "-" / "^" / "_"]]></remarks>
        bool TryMakeRegNameChars(out Token token)
        {
            token = Enumerator.Take();

            switch (token.Kind)
            {
                case TokenKind.Text:
                case TokenKind.Number:
                    return true;

                case TokenKind.Other:
                    var allowable = new [] { '!', '#', '$', '&', '.', '+', '-', '^', '_' };
                    return token.Length == 1 && allowable.Contains((char)token.First());
            }

            return false;
        }

        /// <summary>
        /// Attempt to make a parameter list.
        /// </summary>
        /// <param name="parameters">The parameter list that was made.</param>
        /// <returns>true if the parameter list coud be made, false if not.</returns>
        bool TryMakeOptionalParameterList(out Dictionary<string, string> parameters)
        {
            parameters = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            Enumerator.Skip(TokenKind.Space);

            if (Enumerator.Peek() == Tokens.SemiColon)
            {
                return TryMakeParameterList(out parameters);
            }

            return true;
        }

        /// <summary>
        /// Attempt to make a parameter list.
        /// </summary>
        /// <param name="parameters">The parameter list that was made.</param>
        /// <returns>true if the parameter list coud be made, false if not.</returns>
        bool TryMakeParameterList(out Dictionary<string, string> parameters)
        {
            parameters = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            while (Enumerator.Peek() == Tokens.SemiColon)
            {
                Enumerator.Take();
                Enumerator.Skip(TokenKind.Space);

                if (TryMakeParameter(out string attribute, out string value) == false)
                {
                    return false;
                }

                parameters[attribute] = value;

                Enumerator.Skip(TokenKind.Space);
            }

            return true;
        }

        /// <summary>
        /// Attempt to make a content type parameter.
        /// </summary>
        /// <param name="attribute">The attribute for the parameter.</param>
        /// <param name="value">The value for the parameter.</param>
        /// <returns>true if the parameter could be made, false if not.</returns>
        /// <remarks><![CDATA[attribute "=" value]]></remarks>
        bool TryMakeParameter(out string attribute, out string value)
        {
            value = null;

            if (TryMakeParameterAttribute(out attribute) == false)
            {
                return false;
            }

            Enumerator.Skip(TokenKind.Space);

            if (Enumerator.Take() != Tokens.Equal)
            {
                return false;
            }

            Enumerator.Skip(TokenKind.Space);

            return TryMakeParameterValue(out value);
        }

        /// <summary>
        /// Attempt to make a parameter attribute.
        /// </summary>
        /// <param name="attribute">The name of the attribute that was made.</param>
        /// <returns>true if an attribute could be made, false if not.</returns>
        /// <remarks><![CDATA[token]]></remarks>
        bool TryMakeParameterAttribute(out string attribute)
        {
            return TryMakeToken(out attribute);
        }

        /// <summary>
        /// Attempt to make a parameter value.
        /// </summary>
        /// <param name="value">The value of the attribute that was made.</param>
        /// <returns>true if an parameter value could be made, false if not.</returns>
        /// <remarks><![CDATA[token / quoted-string]]></remarks>
        bool TryMakeParameterValue(out string value)
        {
            return TryMake(TryMakeToken, out value) || TryMakeQuotedString(out value);
        }

        /// <summary>
        /// Attempt to make a token.
        /// </summary>
        /// <param name="token">The token value that was made.</param>
        /// <returns>true if a token value could be made, false if not.</returns>
        /// <remarks><![CDATA[1*<any (US-ASCII) CHAR except SPACE, CTLs, or tspecials>]]></remarks>
        bool TryMakeToken(out string token)
        {
            if (TryMakePartialToken(out token) == false)
            {
                return false;
            }

            while (TryMake(TryMakePartialToken, out string found))
            {
                token += found;
            }

            return true;
        }

        /// <summary>
        /// Attempt to make part of a token.
        /// </summary>
        /// <param name="token">The token value that was made.</param>
        /// <returns>true if part of a token value could be made, false if not.</returns>
        bool TryMakePartialToken(out string token)
        {
            var t = Enumerator.Take();
            token = t.Text();

            switch (t.Kind)
            {
                case TokenKind.Text:
                case TokenKind.Number:
                    return true;

                case TokenKind.None:
                case TokenKind.Space:
                case TokenKind.NewLine:
                    return false;

                case TokenKind.Other:
                    return Tokens.TSpecials.Contains(t) == false && Tokens.CTL.Contains(t) == false;
            }

            return false;
        }

        /// <summary>
        /// Attempt to make a quoted string.
        /// </summary>
        /// <param name="text">The quoted string value that was made.</param>
        /// <returns>true if a quoted string value could be made, false if not.</returns>
        /// <remarks><![CDATA[" 1*token "]]></remarks>
        bool TryMakeQuotedString(out string text)
        {
            text = null;

            if (Enumerator.Take() != Tokens.Quote)
            {
                return false;
            }

            while (Enumerator.Peek() != Tokens.Quote)
            {
                string t;
                if (TryMake(TryMakeQText, out t) == false && TryMake(TryMakeQuotedPair, out t) == false)
                {
                    return false;
                }

                text += t;
            }

            Enumerator.Take();

            return true;
        }

        /// <summary>
        /// Attempt to make a QText string.
        /// </summary>
        /// <param name="text">The QText string value that was made.</param>
        /// <returns>true if a QText string value could be made, false if not.</returns>
        /// <remarks><![CDATA[<any CHAR excepting <">, "\" & CR, and including, linear-white-space>]]></remarks>
        bool TryMakeQText(out string text)
        {
            var token = Enumerator.Take();
            text = token.Text();

            return new[] { Tokens.Quote, Tokens.BackSlash, Tokens.CR }.Contains(token) == false;
        }

        /// <summary>
        /// Attempt to make a quoted pair string.
        /// </summary>
        /// <param name="text">The quoted pair string value that was made.</param>
        /// <returns>true if a quoted pair string value could be made, false if not.</returns>
        /// <remarks><![CDATA[<any CHAR excepting <">, "\" & CR, and including, linear-white-space>]]></remarks>
        bool TryMakeQuotedPair(out string text)
        {
            text = null;

            if (Enumerator.Take() != Tokens.BackSlash)
            {
                return false;
            }

            var token = Enumerator.Take();
            text = token.Text();

            return text.Length == 1 && text[0] <= 127;
        }

        /// <summary>
        /// Attempt to make the end of the line.
        /// </summary>
        /// <returns>true if the end of line could be made, false if not.</returns>
        internal bool TryMakeFieldEnd()
        {
            Enumerator.Skip(TokenKind.Space);

            var token = Enumerator.Take();

            return token.Kind == TokenKind.NewLine || token == Token.None;
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
            while (token != Token.None && token != Tokens.Space && token != Tokens.Colon)
            {
                token = Enumerator.Take();
                switch (token.Kind)
                {
                    case TokenKind.Text:
                    case TokenKind.Number:
                        break;

                    case TokenKind.Space:
                    case TokenKind.Other:
                        if (token.First() <= 31)
                        {
                            return false;
                        }
                        break;

                    default:
                        return false;
                }

                name += token.Text();
                token = Enumerator.Peek();
            }

            return name != null && Enumerator.Take() == Tokens.Colon;
        }

        /// <summary>
        /// Create a stream that returns the underlying list of tokens.
        /// </summary>
        /// <param name="offset">The offset into the list of tokens.</param>
        /// <param name="count">The number of tokens to limit the response to.</param>
        /// <returns>The stream that represents the underlying tokens.</returns>
        Stream CreateTokenStream(int offset, int count)
        {
            var tokens = Enumerator.Tokens.Skip(offset).Take(count);

            return new ByteArrayStream(tokens.SelectMany(t => t.Segments).ToList());
        }
    }
}
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SmtpServer.IO;
using SmtpServer.Mime;
using SmtpServer.Text;

namespace SmtpServer.Mail
{
    public sealed class MimeMessageSerializer : IMessageSerializer
    {
        /// <summary>
        /// Serialize the message to a stream.
        /// </summary>
        /// <param name="message">The message to serialize.</param>
        /// <param name="stream">The stream to serialize the message to.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The stream that the message was serialized to.</returns>
        public Task SerializeAsync(IMessage message, Stream stream, CancellationToken cancellationToken = default(CancellationToken))
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Deserialize a message from the stream.
        /// </summary>
        /// <param name="networkClient">The network client to deserialize the message from.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The message that was deserialized.</returns>
        public async Task<IMessage> DeserializeAsync(INetworkClient networkClient, CancellationToken cancellationToken = default(CancellationToken))
        {
            var entity = await new Deserializer(networkClient, cancellationToken).DeserializeMimeDocumentAsync();

            return null;
        }
        
        #region Deserializer

        class Deserializer
        {
            readonly INetworkClient _networkClient;
            readonly CancellationToken _cancellationToken;

            /// <summary>
            /// Constructor.
            /// </summary>
            /// <param name="networkClient">The network client to deserialize from.</param>
            /// <param name="cancellationToken">The cancellation token.</param>
            public Deserializer(INetworkClient networkClient, CancellationToken cancellationToken)
            {
                _networkClient = networkClient;
                _cancellationToken = cancellationToken;
            }

            /// <summary>
            /// Read a MIME entity from the current position in the stream.
            /// </summary>
            /// <returns>The MIME entity that was read from the stream.</returns>
            public async Task<MimeDocument> DeserializeMimeDocumentAsync()
            {
                var entity = await DeserializeMimeEntityAsync();

                throw new NotSupportedException();
            }

            /// <summary>
            /// Read a MIME entity from the current position in the stream.
            /// </summary>
            /// <returns>The MIME entity that was read from the stream.</returns>
            public async Task<MimeEntity> DeserializeMimeEntityAsync()
            {
                var headers = await DeserializeMimeHeadersAsync().ReturnOnAnyThread();

                switch (GetMediaType(headers).ToLower())
                {
                    case "multipart":
                        break;

                    case "message":
                        break;

                    default:
                        return await DeserializeMimePartAsync(headers);
                }

                throw new NotSupportedException();
            }

            /// <summary>
            /// Returns the media type that has been specified in the headers.
            /// </summary>
            /// <param name="headers">The list of headers that could possible contain the media type.</param>
            /// <returns>The media type that has been defined.</returns>
            static string GetMediaType(IReadOnlyList<IMimeHeader> headers)
            {
                // TODO: check the MIME-Version to make sure the content type 
                // should exist otherwise allow for a default version type

                var type = headers.OfType<ContentType>().Last();

                return type.MediaType;
            }

            /// <summary>
            /// Deserialize a MIME part from the current position in the stream.
            /// </summary>
            /// <param name="headers">The list of headers to create the MIME part with.</param>
            /// <returns>The MIME part that was created from the current position in the stream.</returns>
            async Task<MimePart> DeserializeMimePartAsync(IReadOnlyList<IMimeHeader> headers)
            {
                var transferEncoding = headers.OfType<ContentTransferEncoding>().SingleOrDefault() ?? ContentTransferEncoding.SevenBit;

                return new MimePart(headers, await DeserializeMimePartAsync(transferEncoding));
            }

            /// <summary>
            /// Deserialize a MIME part from the current position in the stream.
            /// </summary>
            /// <param name="transferEncoding">The transfer encoding to use when reading the message contents.</param>
            /// <returns>The MIME part that was created from the current position in the stream.</returns>
            async Task<Stream> DeserializeMimePartAsync(ContentTransferEncoding transferEncoding)
            {
                if (transferEncoding == ContentTransferEncoding.SevenBit)
                {
                    return await ReadPlainTextAsync(Encoding.ASCII).ReturnOnAnyThread();
                }

                if (transferEncoding == ContentTransferEncoding.EightBit)
                {
                    return await ReadPlainTextAsync(Encoding.UTF8).ReturnOnAnyThread();
                }

                throw new NotImplementedException();
            }

            /// <summary>
            /// Read an unencoded/plain text message from the current position in the stream.
            /// </summary>
            /// <param name="textEncoding">The text encoding to apply to the text mesage.</param>
            /// <returns>The stream that represents the message contents that was read.</returns>
            async Task<Stream> ReadPlainTextAsync(Encoding textEncoding)
            {
                // TODO: need to handle dot-stuffing here

                return new ByteArrayStream(await _networkClient.ReadUntilAsync(new byte[] { 13, 10, 46, 13, 10 }, _cancellationToken));
            }

            /// <summary>
            /// Read the MIME headers.
            /// </summary>
            /// <returns>The list of MIME headers that were read.</returns>
            async Task<IReadOnlyList<IMimeHeader>> DeserializeMimeHeadersAsync()
            {
                var tokens = await ReadMimeHeaderTokensAsync().ReturnOnAnyThread();

                var mimeParser = new MimeParser(new TokenEnumerator(tokens));

                if (mimeParser.TryMakeFieldList(out List<IMimeHeader> headers) == false)
                {
                    throw new MimeParseException("Could not match the MIME headers.");
                }

                return headers;
            }

            /// <summary>
            /// Read the MIME headers.
            /// </summary>
            /// <returns>The list of MIME headers that were read.</returns>
            async Task<IReadOnlyList<Token>> ReadMimeHeaderTokensAsync()
            {
                var tokenReader = new ByteArrayTokenReader(await _networkClient.ReadBlockAsync(_cancellationToken).ReturnOnAnyThread());

                //https://tools.ietf.org/html/rfc6531
                //https://tools.ietf.org/html/rfc6532

                return tokenReader.ToList();
            }
        }

        #endregion
    }
}
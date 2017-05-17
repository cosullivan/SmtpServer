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
        /// Deserialize a message from the stream.
        /// </summary>
        /// <param name="networkClient">The network client to deserialize the message from.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The message that was deserialized.</returns>
        public async Task<IMessage> DeserializeAsync(INetworkClient networkClient, CancellationToken cancellationToken = default(CancellationToken))
        {
           var segments = await networkClient.ReadDotBlockAsync(cancellationToken);

            try
            {
                var parser = new MimeParser(new TokenEnumerator(new ByteArrayTokenReader(segments)));

                if (parser.TryMakeDocument(out MimeDocument document) == false) 
                {
                    return new TextMessage(new ByteArrayStream(segments));
                }

                return new MimeMessage(document);
            }
            catch
            {
                // fallback to a standard text message
                return new TextMessage(new ByteArrayStream(segments));
            }
        }

        ///// <summary>
        ///// Deserialize a message from the stream.
        ///// </summary>
        ///// <param name="networkClient">The network client to deserialize the message from.</param>
        ///// <param name="cancellationToken">The cancellation token.</param>
        ///// <returns>The message that was deserialized.</returns>
        //internal Task<MimeDocument> DeserializeDocumentAsync(INetworkClient networkClient, CancellationToken cancellationToken = default(CancellationToken))
        //{
        //    return new Deserializer(networkClient, cancellationToken).DeserializeMimeDocumentAsync();
        //}

        //#region Deserializer

        //class Deserializer
        //{
        //    readonly INetworkClient _networkClient;
        //    readonly CancellationToken _cancellationToken;

        //    /// <summary>
        //    /// Constructor.
        //    /// </summary>
        //    /// <param name="networkClient">The network client to deserialize from.</param>
        //    /// <param name="cancellationToken">The cancellation token.</param>
        //    public Deserializer(INetworkClient networkClient, CancellationToken cancellationToken)
        //    {
        //        _networkClient = networkClient;
        //        _cancellationToken = cancellationToken;
        //    }

        //    /// <summary>
        //    /// Read a MIME entity from the current position in the stream.
        //    /// </summary>
        //    /// <returns>The MIME entity that was read from the stream.</returns>
        //    public async Task<MimeDocument> DeserializeMimeDocumentAsync()
        //    {
        //        var entity = await DeserializeMimeEntityAsync();

        //        return new MimeDocument(entity.HeaderOrDefault<MimeVersion>(), entity);
        //    }

        //    /// <summary>
        //    /// Read a MIME entity from the current position in the stream.
        //    /// </summary>
        //    /// <returns>The MIME entity that was read from the stream.</returns>
        //    public async Task<MimeEntity> DeserializeMimeEntityAsync()
        //    {
        //        var headers = await DeserializeMimeHeadersAsync().ReturnOnAnyThread();

        //        // TODO: check to ensure that this is a MIME-Version: 1.0 ?
        //        var contentType = headers.OfType<ContentType>().Last();

        //        switch (contentType.MediaType.ToLower())
        //        {
        //            case "multipart":
        //                var x = await DeserializeMultiPartAsync(headers, contentType);
        //                break;

        //            case "message":
        //                break;

        //            default:
        //                return await DeserializeMimePartAsync(headers);
        //        }

        //        throw new NotSupportedException();
        //    }

        //    /// <summary>
        //    /// Deserialize a MIME part from the current position in the stream.
        //    /// </summary>
        //    /// <param name="headers">The list of headers to create the MIME part with.</param>
        //    /// <returns>The MIME part that was created from the current position in the stream.</returns>
        //    async Task<MimePart> DeserializeMimePartAsync(IReadOnlyList<IMimeHeader> headers)
        //    {
        //        var transferEncoding = headers.OfType<ContentTransferEncoding>().SingleOrDefault() ?? ContentTransferEncoding.SevenBit;

        //        return new MimePart(headers, await DeserializeMimePartAsync(transferEncoding));
        //    }

        //    /// <summary>
        //    /// Deserialize a MIME part from the current position in the stream.
        //    /// </summary>
        //    /// <param name="transferEncoding">The transfer encoding to use when reading the message contents.</param>
        //    /// <returns>The MIME part that was created from the current position in the stream.</returns>
        //    async Task<Stream> DeserializeMimePartAsync(ContentTransferEncoding transferEncoding)
        //    {
        //        if (transferEncoding == ContentTransferEncoding.SevenBit)
        //        {
        //            return await ReadPlainTextAsync().ReturnOnAnyThread();
        //        }

        //        if (transferEncoding == ContentTransferEncoding.EightBit)
        //        {
        //            return await ReadPlainTextAsync().ReturnOnAnyThread();
        //        }

        //        throw new NotImplementedException();
        //    }

        //    async Task<MimeMultipart> DeserializeMultiPartAsync(IReadOnlyList<IMimeHeader> headers, ContentType contentType)
        //    {
        //        var boundary = new byte[] { 13, 10, 45, 45 }.Union(Encoding.ASCII.GetBytes(contentType.Parameters["boundary"])).ToArray();

        //        await _networkClient.ReadUntilAsync(boundary, _cancellationToken);

        //        return null;
        //    }

        //    /// <summary>
        //    /// Read an unencoded/plain text message from the current position in the stream.
        //    /// </summary>
        //    /// <returns>The stream that represents the message contents that was read.</returns>
        //    async Task<Stream> ReadPlainTextAsync()
        //    {
        //        return new ByteArrayStream(await _networkClient.ReadUntilAsync(new byte[] { 13, 10, 46, 13, 10 }, _cancellationToken));
        //    }

        //    /// <summary>
        //    /// Read the MIME headers.
        //    /// </summary>
        //    /// <returns>The list of MIME headers that were read.</returns>
        //    async Task<IReadOnlyList<IMimeHeader>> DeserializeMimeHeadersAsync()
        //    {
        //        var blocks = await _networkClient.ReadBlockAsync(_cancellationToken).ReturnOnAnyThread();

        //        var mimeParser = new MimeParser(new TokenEnumerator(new ByteArrayTokenReader(blocks)));
        //        if (mimeParser.TryMakeFieldList(out List<IMimeHeader> headers) == false)
        //        {
        //            throw new MimeParseException("Could not match the MIME headers.");
        //        }

        //        return headers;
        //    }
        //}

        //#endregion
    }
}
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SmtpServer.Mime;
using SmtpServer.Text;

namespace SmtpServer.Content
{
    public interface IMessageSerializer
    {
        /// <summary>
        /// Serialize the message to a stream.
        /// </summary>
        /// <param name="message">The message to serialize.</param>
        /// <param name="stream">The stream to serialize the message to.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The stream that the message was serialized to.</returns>
        Task SerializeAsync(IMessage message, Stream stream, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Deserialize a message from the stream.
        /// </summary>
        /// <param name="stream">The stream to deserialize the message from.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The message that was deserialized.</returns>
        Task<IMessage> DeserializeAsync(Stream stream, CancellationToken cancellationToken = default(CancellationToken));
    }

    public sealed class MimeMessageReader : IMessageReader, IMessageSerializer
    {
        readonly Stream _stream;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="stream">The stream that is being read from.</param>
        public MimeMessageReader(Stream stream)
        {
            _stream = stream;
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        public MimeMessageReader() { }

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
        /// <param name="stream">The stream to deserialize the message from.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The message that was deserialized.</returns>
        public async Task<IMessage> DeserializeAsync(Stream stream, CancellationToken cancellationToken = default(CancellationToken))
        {
            var entity = await new Deserializer(stream, cancellationToken).DeserializeMimeEntityAsync();

            return null;
        }

        /// <summary>
        /// Read a message 
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The message that was read from the stream.</returns>
        public async Task<IMessage> ReadAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            //return await ReadMessageAsync(cancellationToken);
            throw new NotImplementedException();
        }
        
        #region Deserializer

        class Deserializer
        {
            readonly StreamReader2 _reader;
            readonly CancellationToken _cancellationToken;

            /// <summary>
            /// Constructor.
            /// </summary>
            /// <param name="stream">The stream to deserialize from.</param>
            /// <param name="cancellationToken">The cancellation token.</param>
            public Deserializer(Stream stream, CancellationToken cancellationToken)
            {
                _reader = new StreamReader2(stream);
                _cancellationToken = cancellationToken;
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
                var stream = await ReadShortLineContentAsync(textEncoding).ReturnOnAnyThread();
                stream.Position = 0;

                return stream;
            }

            /// <summary>
            /// Receive the message content in short line format.
            /// </summary>
            /// <param name="encoding">The encoding to use when reading the message content.</param>
            /// <returns>A task which asynchronously performs the operation.</returns>
            async Task<Stream> ReadShortLineContentAsync(Encoding encoding)
            {
                //var reader = new StreamReader(_stream, encoding);
                //var writer = new StreamWriter(new MemoryStream(), encoding);

                //try
                //{
                //    string text;
                //    while ((text = await reader.ReadLineAsync(TimeSpan.FromSeconds(60), _cancellationToken).ReturnOnAnyThread()) != ".")
                //    {
                //        // need to trim the '.' at the start of the line if it 
                //        // exists as this would have been added for transparency
                //        // http://tools.ietf.org/html/rfc5321#section-4.5.2
                //        writer.WriteLine(text.TrimStart('.'));
                //        writer.Flush();
                //    }
                //}
                //catch (TimeoutException)
                //{
                //    // TODO: not sure what the best thing to do here is
                //    throw;
                //}

                //return writer.BaseStream;

                //var reader = new StreamReader(_stream, encoding);
                var writer = new StreamWriter(new MemoryStream(), encoding);

                //HERE: maybe the simple version here is some sort of "MimeStreamReader" or "BlockStreamReader"
                //that can read the different parts as they are probably all terminated by the same endings?
                //it would need the ability to change encoding, or perhaps not if the delimeters are the same?

                try
                {
                    
                }
                catch (TimeoutException)
                {
                    // TODO: not sure what the best thing to do here is
                    throw;
                }

                return writer.BaseStream;
            }

            /// <summary>
            /// Read the MIME headers.
            /// </summary>
            /// <returns>The list of MIME headers that were read.</returns>
            async Task<IReadOnlyList<IMimeHeader>> DeserializeMimeHeadersAsync()
            {
                var tokens = await ReadMimeHeaderTokensAsync().ReturnOnAnyThread();

                var mimeParser = new MimeParser(new TokenEnumerator2(tokens));

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
                var tokenReader = new ByteArrayTokenReader(await ReadMimeContentBlockAsync().ReturnOnAnyThread());

                //https://tools.ietf.org/html/rfc6531
                //https://tools.ietf.org/html/rfc6532

                var tokens = tokenReader.ToList();
                foreach (var token in tokens)
                {
                    Console.WriteLine(token);
                }
                //var reader = new StreamTokenReader(_stream, Encoding.UTF8);

                //Token token;
                //while ((token = await reader.NextTokenAsync(_cancellationToken)) != Token.None)
                //{
                //    if (tokens.Count > 1 && tokens[tokens.Count - 1] == Token.NewLine && token == Token.NewLine)
                //    {
                //        return tokens;
                //    }

                //    tokens.Add(token);
                //}

                return tokens;
            }

            /// <summary>
            /// Read a content block that is completed by a null line.
            /// </summary>
            /// <returns>The stream that makes up the content block.</returns>
            async Task<IReadOnlyList<ArraySegment<byte>>> ReadMimeContentBlockAsync()
            {
                // ReSharper disable InconsistentNaming
                const int WaitForFirstCR = 0;
                const int WaitForFirstLF = 1;
                const int WaitForSecondCR = 2;
                const int WaitForSecondLF = 3;
                const int Terminate = 4;
                // ReSharper restore InconsistentNaming

                var state = WaitForFirstCR;
                return await _reader.ReadWhileAsync(current =>
                {
                    switch (state)
                    {
                        case WaitForFirstCR:
                            if (current == 13)
                            {
                                state = WaitForFirstLF;
                            }
                            break;

                        case WaitForFirstLF:
                            state = current == 10 ? WaitForSecondCR : WaitForFirstCR;
                            break;

                        case WaitForSecondCR:
                            state = current == 13 ? WaitForSecondLF : WaitForFirstCR;
                            break;

                        case WaitForSecondLF:
                            state = current == 10 ? Terminate : WaitForFirstCR;
                            break;

                        case Terminate:
                            return false;
                    }

                    return true;
                }, 
                _cancellationToken);
            }
        }

        #endregion
    }

    //public sealed class ByteArrayStream : Stream
    //{
    //    readonly IReadOnlyList<ArraySegment<byte>> _segments;
    //    readonly int _length;

    //    /// <summary>
    //    /// Constructor.
    //    /// </summary>
    //    /// <param name="segments">The list of array segments to provide the data for the stream.</param>
    //    public ByteArrayStream(IReadOnlyList<ArraySegment<byte>> segments)
    //    {
    //        _segments = segments;
    //        _length = segments.Sum(s => s.Count);
    //    }

    //    /// <summary>
    //    /// When overridden in a derived class, clears all buffers for this stream and causes any buffered data to be written to the underlying device.
    //    /// </summary>
    //    public override void Flush()
    //    {
    //        throw new NotImplementedException();
    //    }

    //    /// <summary>
    //    /// When overridden in a derived class, sets the position within the current stream.
    //    /// </summary>
    //    /// <param name="offset">A byte offset relative to the <paramref name="origin" /> parameter. </param>
    //    /// <param name="origin">A value of type <see cref="T:System.IO.SeekOrigin" /> indicating the reference point used to obtain the new position. </param>
    //    /// <returns>The new position within the current stream.</returns>
    //    public override long Seek(long offset, SeekOrigin origin)
    //    {
    //        throw new NotImplementedException();
    //    }

    //    /// <summary>
    //    /// When overridden in a derived class, sets the length of the current stream.
    //    /// </summary>
    //    /// <param name="value">The desired length of the current stream in bytes. </param>
    //    public override void SetLength(long value)
    //    {
    //        throw new NotImplementedException();
    //    }

    //    /// <summary>
    //    /// When overridden in a derived class, reads a sequence of bytes from the current stream and advances the position within the stream by the number of bytes read.
    //    /// </summary>
    //    /// <returns>The total number of bytes read into the buffer. This can be less than the number of bytes requested if that many bytes are not currently available, or zero (0) if the end of the stream has been reached.</returns>
    //    /// <param name="buffer">An array of bytes. When this method returns, the buffer contains the specified byte array with the values between <paramref name="offset" /> and (<paramref name="offset" /> + <paramref name="count" /> - 1) replaced by the bytes read from the current source. </param>
    //    /// <param name="offset">The zero-based byte offset in <paramref name="buffer" /> at which to begin storing the data read from the current stream. </param>
    //    /// <param name="count">The maximum number of bytes to be read from the current stream. </param>
    //    public override int Read(byte[] buffer, int offset, int count)
    //    {
    //        throw new NotImplementedException();
    //    }

    //    /// <summary>
    //    /// Reads a byte from the stream and advances the position within the stream by one byte, or returns -1 if at the end of the stream.
    //    /// </summary>
    //    /// <returns>The unsigned byte cast to an Int32, or -1 if at the end of the stream.</returns>
    //    public override int ReadByte()
    //    {
    //        return base.ReadByte();
    //    }

    //    /// <summary>
    //    /// When overridden in a derived class, writes a sequence of bytes to the current stream and advances the current position within this stream by the number of bytes written.
    //    /// </summary>
    //    /// <param name="buffer">An array of bytes. This method copies <paramref name="count" /> bytes from <paramref name="buffer" /> to the current stream. </param>
    //    /// <param name="offset">The zero-based byte offset in <paramref name="buffer" /> at which to begin copying bytes to the current stream. </param>
    //    /// <param name="count">The number of bytes to be written to the current stream. </param>
    //    public override void Write(byte[] buffer, int offset, int count)
    //    {
    //        throw new NotImplementedException();
    //    }

    //    /// <summary>
    //    /// When overridden in a derived class, gets a value indicating whether the current stream supports reading.
    //    /// </summary>
    //    /// <returns>true if the stream supports reading; otherwise, false.</returns>
    //    public override bool CanRead => true;

    //    /// <summary>
    //    /// When overridden in a derived class, gets a value indicating whether the current stream supports seeking.
    //    /// </summary>
    //    /// <returns>true if the stream supports seeking; otherwise, false.</returns>
    //    public override bool CanSeek => true;

    //    /// <summary>
    //    /// When overridden in a derived class, gets a value indicating whether the current stream supports writing.
    //    /// </summary>
    //    /// <returns>true if the stream supports writing; otherwise, false.</returns>
    //    public override bool CanWrite => false;

    //    /// <summary>
    //    /// When overridden in a derived class, gets the length in bytes of the stream.
    //    /// </summary>
    //    /// <returns>A long value representing the length of the stream in bytes.</returns>
    //    public override long Length => _length;

    //    /// <summary>
    //    /// When overridden in a derived class, gets or sets the position within the current stream.
    //    /// </summary>
    //    /// <returns>The current position within the stream.</returns>
    //    public override long Position { get; set; }
    //}
}
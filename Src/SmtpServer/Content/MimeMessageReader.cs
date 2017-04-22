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
    public sealed class MimeMessageReader : IMessageReader
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
        /// Read a message 
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The message that was read from the stream.</returns>
        public async Task<IMessage> ReadAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            return await ReadMessageAsync(cancellationToken);
        }

        /// <summary>
        /// Read the contents of the message.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The MIME message that was read.</returns>
        async Task<MimeMessage> ReadMessageAsync(CancellationToken cancellationToken)
        {
            var headers = await ReadMimeHeadersAsync(cancellationToken);

            return null;
        }

        /// <summary>
        /// Read the MIME headers.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The list of MIME headers that were read.</returns>
        async Task<IEnumerable<IMimeHeader>> ReadMimeHeadersAsync(CancellationToken cancellationToken)
        {
            var tokens = await ReadMimeHeaderTokensAsync(cancellationToken);
            cancellationToken.ThrowIfCancellationRequested();

            //tokens = tokens.Where(t => t.Kind != TokenKind.Space).ToList();

            var mimeParser = new MimeParser(new TokenEnumerator2(tokens));
            //mimeParser.TryMakeMimeVersion(out MimeVersion version);
            //Console.WriteLine(version);

            var result = mimeParser.TryMakeField(out IMimeHeader header);
            Console.WriteLine(result);
            Console.WriteLine(header);

            return null;
        }

        /// <summary>
        /// Read the MIME headers.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The list of MIME headers that were read.</returns>
        async Task<IReadOnlyList<Token>> ReadMimeHeaderTokensAsync(CancellationToken cancellationToken)
        {
            var tokens = new List<Token>();
            var reader = new StreamTokenReader(_stream);

            Token token;
            while ((token = await reader.NextTokenAsync(cancellationToken)) != Token.None)
            {
                if (tokens.Count > 1 && tokens[tokens.Count - 1] == Token.NewLine && token == Token.NewLine)
                {
                    return tokens;
                }

                tokens.Add(token);
            }

            return tokens;
        }
    }
}
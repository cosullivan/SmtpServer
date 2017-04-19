using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SmtpServer.Mime;

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

        Task<MimeMessage> ReadMessageAsync(CancellationToken cancellationToken)
        {
            var reader = new StreamReader(_stream, Encoding.UTF8);
            //reader.ReadLineAsync()
            //var headers = await ReadMessageHeadersAsync(cancellationToken);

            return null;
        }

        Task<IEnumerable<IMimeHeader>> ReadMimeHeadersAsync(CancellationToken cancellationToken)
        {
            return null;
        }
        
        //async Task<IReadOnlyList<MimeHeader>> ReadMessageHeadersAsync(CancellationToken cancellationToken)
        //{
        //    HERE: implement the StreamTokenReader which understands the tokens from
        //    the stream and potentially knows about he End of the header section and body section?

        //        Needs to be a ReadNextAsync() 


        //    var tokenReader = new StreamTokenReader(_stream, ignoreWhiteSpace: true);
        //    //var headers = await ReadMessageHeadersAsync(cancellationToken);
        //    var mimeParser = new MimeParser(tokenReader);

        //    if (mimeParser.TryMakeMimeHeaders(out IReadOnlyList<MimeHeader> headers))
        //    {
        //        return headers;
        //    }

        //    return null;
        //}
    }
}
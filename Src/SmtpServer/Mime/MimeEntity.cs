using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SmtpServer.Mime
{
    public abstract class MimeEntity
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="headers">The list of headers for the entity.</param>
        protected MimeEntity(IReadOnlyList<IMimeHeader> headers)
        {
            Headers = headers;
        }

        /// <summary>
        /// The content type.
        /// </summary>
        public ContentType ContentType => Headers.OfType<ContentType>().Last();

        /// <summary>
        /// The list of headers for the entity.
        /// </summary>
        public IReadOnlyList<IMimeHeader> Headers { get; }
    }

    public sealed class MimePart : MimeEntity
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="headers">The list of headers for the entity.</param>
        /// <param name="content">The contents of the MIME part.</param>
        public MimePart(IReadOnlyList<IMimeHeader> headers, Stream content) : base(headers)
        {
            Content = content;
        }

        /// <summary>
        /// The contents of the MIME part.
        /// </summary>
        public Stream Content { get; }
    }

    public sealed class MimeMultipart { }

    public sealed class MimeMessagePart { }
}
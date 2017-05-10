using System.Collections.Generic;
using System.IO;

namespace SmtpServer.Mime
{
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
}
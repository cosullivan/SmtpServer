using System;
using System.Collections.Generic;
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

    public static class MimeEntityExtensions
    {
        /// <summary>
        /// Return the header of the specified type.
        /// </summary>
        /// <typeparam name="THeader">The type of the header to return.</typeparam>
        /// <param name="entity">The entity that contains the headers.</param>
        /// <returns>The header of the specified type.</returns>
        public static THeader HeaderOrDefault<THeader>(this MimeEntity entity) where THeader : IMimeHeader
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            return entity.Headers.OfType<THeader>().SingleOrDefault();
        }
    }
}
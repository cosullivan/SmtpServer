namespace SmtpServer.Mime
{
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
}
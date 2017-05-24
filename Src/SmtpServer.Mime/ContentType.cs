using System.Collections.Generic;

namespace SmtpServer.Mime
{
    public sealed class ContentType : IMimeHeader
    {
        public static readonly ContentType TextPlain = new ContentType("text", "plain", new Dictionary<string, string>());

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="mediaType">The media type.</param>
        /// <param name="mediaSubType">The subtype.</param>
        /// <param name="parameters">The list of parameters.</param>
        public ContentType(string mediaType, string mediaSubType, IReadOnlyDictionary<string, string> parameters)
        {
            MediaType = mediaType;
            MediaSubType = mediaSubType;
            Parameters = parameters;
        }

        /// <summary>
        /// The name of the header.
        /// </summary>
        public string Name => "Content-Type";

        /// <summary>
        /// The media type.
        /// </summary>
        public string MediaType { get; }

        /// <summary>
        /// The subtype.
        /// </summary>
        public string MediaSubType { get; }

        /// <summary>
        /// The list of parameters.
        /// </summary>
        public IReadOnlyDictionary<string, string> Parameters { get; }
    }
}
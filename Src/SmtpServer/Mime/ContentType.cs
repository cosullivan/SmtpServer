using System.Collections.Generic;

namespace SmtpServer.Mime
{
    public sealed class ContentType : IMimeHeader
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="type">The media type.</param>
        /// <param name="subtype">The subtype.</param>
        /// <param name="parameters">The list of parameters.</param>
        public ContentType(string type, string subtype, IReadOnlyDictionary<string, string> parameters)
        {
            Type = type;
            SubType = subtype;
            Parameters = parameters;
        }

        /// <summary>
        /// The name of the header.
        /// </summary>
        public string Name => "Content-Type";

        /// <summary>
        /// The media type.
        /// </summary>
        public string Type { get; }

        /// <summary>
        /// The subtype.
        /// </summary>
        public string SubType { get; }

        /// <summary>
        /// The list of parameters.
        /// </summary>
        public IReadOnlyDictionary<string, string> Parameters { get; }
    }
}
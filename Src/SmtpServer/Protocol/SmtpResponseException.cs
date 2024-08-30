using System;
using System.Collections.Generic;

namespace SmtpServer.Protocol
{
    /// <summary>
    /// Smtp Response Exception
    /// </summary>
    public sealed class SmtpResponseException : Exception
    {
        static readonly IReadOnlyDictionary<string, object> Empty = new Dictionary<string, object>();

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="response">The response to raise in the exception.</param>
        public SmtpResponseException(SmtpResponse response) : this(response, false) { }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="response">The response to raise in the exception.</param>
        /// <param name="quit">Indicates whether or not the session should terminate.</param>
        public SmtpResponseException(SmtpResponse response, bool quit) : this(response, quit, Empty) { }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="response">The response to raise in the exception.</param>
        /// <param name="quit">Indicates whether or not the session should terminate.</param>
        /// <param name="properties">The contextual properties to include as metadata for the exception.</param>
        public SmtpResponseException(SmtpResponse response, bool quit, IReadOnlyDictionary<string, object> properties)
        {
            Response = response;
            IsQuitRequested = quit;
            Properties = properties;
        }

        /// <summary>
        /// The response to return to the client.
        /// </summary>
        public SmtpResponse Response { get; }

        /// <summary>
        /// Indicates whether or not the session should terminate.
        /// </summary>
        public bool IsQuitRequested { get; }

        /// <summary>
        /// Returns a set of propeties for the current session.
        /// </summary>
        public IReadOnlyDictionary<string, object> Properties { get; }
    }
}

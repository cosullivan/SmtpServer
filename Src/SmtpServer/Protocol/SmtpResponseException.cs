using System;

namespace SmtpServer.Protocol
{
    public sealed class SmtpResponseException : Exception
    {
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
        public SmtpResponseException(SmtpResponse response, bool quit)
        {
            Response = response;
            IsQuitRequested = quit;
        }

        /// <summary>
        /// The response to return to the client.
        /// </summary>
        public SmtpResponse Response { get; }

        /// <summary>
        /// Indicates whether or not the session should terminate.
        /// </summary>
        public bool IsQuitRequested { get; }
    }
}
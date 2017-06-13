using System;

namespace SmtpServer.Protocol
{
    public sealed class SmtpResponseException : Exception
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="response">The response to raise in the exception.</param>
        public SmtpResponseException(SmtpResponse response)
        {
            Response = response;
        }

        /// <summary>
        /// The response to return to the client.
        /// </summary>
        public SmtpResponse Response { get; }
    }
}
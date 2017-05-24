using System;

namespace SmtpServer.Mime
{
    public sealed class MimeParseException : Exception
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="format">The message format.</param>
        /// <param name="args">The message arguments.</param>
        public MimeParseException(string format, params object[] args) : base(String.Format(format, args)) { }
    }
}
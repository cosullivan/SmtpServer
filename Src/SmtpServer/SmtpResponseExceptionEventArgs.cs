using SmtpServer.Protocol;

namespace SmtpServer
{
    /// <summary>
    /// Smtp Response Exception EventArgs
    /// </summary>
    public sealed class SmtpResponseExceptionEventArgs : SessionEventArgs
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="context">The session context.</param>
        /// <param name="exception">The exception that occured</param>
        public SmtpResponseExceptionEventArgs(ISessionContext context, SmtpResponseException exception) : base(context)
        {
            Exception = exception;
        }

        /// <summary>
        /// Returns the exception.
        /// </summary>
        public SmtpResponseException Exception { get; }
    }
}

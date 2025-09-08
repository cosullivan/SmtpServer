using System;

namespace SmtpServer
{
    /// <summary>
    /// Session Faulted EventArgs
    /// </summary>
    public class SessionFaultedEventArgs : SessionEventArgs
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="context">The session context.</param>
        /// <param name="exception">The exception that occured</param>
        public SessionFaultedEventArgs(ISessionContext context, Exception exception) : base(context)
        {
            Exception = exception;
        }

        /// <summary>
        /// Returns the exception.
        /// </summary>
        public Exception Exception { get; }
    }
}
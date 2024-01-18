using System;

namespace SmtpServer
{
    public class ListenerFaultedEventArgs : EventArgs
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="exception">The exception that occured</param>
        public ListenerFaultedEventArgs(Exception exception)
        {
            Exception = exception;
        }

        /// <summary>
        /// Returns the exception.
        /// </summary>
        public Exception Exception { get; }
    }
}

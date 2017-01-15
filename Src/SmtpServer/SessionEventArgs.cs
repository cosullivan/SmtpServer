using System;

namespace SmtpServer
{
    public sealed class SessionEventArgs : EventArgs
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="context">The session context.</param>
        public SessionEventArgs(ISessionContext context)
        {
            Context = context;
        }

        /// <summary>
        /// Returns the session context.
        /// </summary>
        public ISessionContext Context { get; }
    }
}
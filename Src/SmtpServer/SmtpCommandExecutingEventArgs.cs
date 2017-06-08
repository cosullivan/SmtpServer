using SmtpServer.Protocol;

namespace SmtpServer
{
    public class SmtpCommandExecutingEventArgs : SmtpCommandEventArgs
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="context">The session context.</param>
        /// <param name="command">The command for the event.</param>
        public SmtpCommandExecutingEventArgs(ISessionContext context, SmtpCommand command) : base(context, command) { }
    }
}
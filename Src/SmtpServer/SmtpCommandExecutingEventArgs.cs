using SmtpServer.Protocol;

namespace SmtpServer
{
    public class SmtpCommandExecutingEventArgs : SmtpCommandEventArgs
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="command">The command for the event.</param>
        public SmtpCommandExecutingEventArgs(SmtpCommand command) : base(command) { }
    }
}
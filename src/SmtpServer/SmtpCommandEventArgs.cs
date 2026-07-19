using SmtpServer.Protocol;

namespace SmtpServer
{
    /// <summary>
    /// Smtp Command EventArgs
    /// </summary>
    public sealed class SmtpCommandEventArgs : SessionEventArgs
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="context">The session context.</param>
        /// <param name="command">The command for the event.</param>
        public SmtpCommandEventArgs(ISessionContext context, SmtpCommand command) : base(context)
        {
            Command = command;
            SafeCommand = SmtpCommandSnapshot.From(command);
        }

        /// <summary>
        /// The command for the event.
        /// </summary>
        public SmtpCommand Command { get; }

        /// <summary>
        /// The safe-to-log command snapshot for the event.
        /// </summary>
        public SmtpCommandSnapshot SafeCommand { get; }
    }
}

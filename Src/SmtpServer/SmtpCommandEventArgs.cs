using System;
using SmtpServer.Protocol;

namespace SmtpServer
{
    public class SmtpCommandEventArgs : EventArgs
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="command">The command for the event.</param>
        public SmtpCommandEventArgs(SmtpCommand command)
        {
            Command = command;
        }

        /// <summary>
        /// The command for the event.
        /// </summary>
        public SmtpCommand Command { get; }
    }
}
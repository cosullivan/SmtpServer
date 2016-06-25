using System.Net;
using SmtpServer.Protocol;

namespace SmtpServer
{
    public interface ISmtpSessionContext : ISessionContext
    {
        /// <summary>
        /// Indicates to the session that it should quit.
        /// </summary>
        void Quit();

        /// <summary>
        /// Gets or sets the text stream to read from and write to.
        /// </summary>
        ITextStream Text { get; set; }

        /// <summary>
        /// Gets the current transaction.
        /// </summary>
        ISmtpTransaction Transaction { get; }

        /// <summary>
        /// Gets the current state machine.
        /// </summary>
        ISmtpStateMachine StateMachine { get; }
    }
}
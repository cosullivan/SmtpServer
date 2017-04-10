using SmtpServer.Protocol;

namespace SmtpServer
{
    internal interface ISmtpSessionContext : ISessionContext
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
        /// The transfer encoding that is required for the message.
        /// </summary>
        ContentTransferEncoding TransferEncoding { get; set; }

        /// <summary>
        /// Gets the current transaction.
        /// </summary>
        ISmtpMessageTransaction Transaction { get; }

        /// <summary>
        /// Gets the current state machine.
        /// </summary>
        ISmtpStateMachine StateMachine { get; }

        /// <summary>
        /// Gets a value indicating whether a quit has been requested.
        /// </summary>
        bool IsQuitRequested { get; }
    }
}
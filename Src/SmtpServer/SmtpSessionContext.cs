namespace SmtpServer
{
    internal class SmtpSessionContext : ISmtpSessionContext
    {
        readonly ISmtpTransaction _transaction;
        bool _isQuitRequested;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="transaction">The SMTP transaction.</param>
        internal SmtpSessionContext(ISmtpTransaction transaction)
        {
            _transaction = transaction;
        }

        /// <summary>
        /// Indicates to the session that it should quit.
        /// </summary>
        public void Quit()
        {
            _isQuitRequested = true;
        }

        /// <summary>
        /// Gets or sets the text stream to read from and write to.
        /// </summary>
        public ITextStream Text { get; set; }

        /// <summary>
        /// Gets the current transaction.
        /// </summary>
        public ISmtpTransaction Transaction
        {
            get { return _transaction; }
        }

        /// <summary>
        /// Gets a value indicating whether a quit has been requested.
        /// </summary>
        internal bool IsQuitRequested
        {
            get { return _isQuitRequested; }
        }
    }
}
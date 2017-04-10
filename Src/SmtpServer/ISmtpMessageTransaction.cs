namespace SmtpServer
{
    internal interface ISmtpMessageTransaction : IMessageTransaction
    {
        /// <summary>
        /// Reset the current transaction.
        /// </summary>
        void Reset();
    }
}
using System;

namespace SmtpServer.Protocol.Text
{
    internal interface ITokenEnumeratorCheckpoint : IDisposable
    {
        /// <summary>
        /// Rollback to the previous position in the numerator.
        /// </summary>
        void Rollback();
    }
}
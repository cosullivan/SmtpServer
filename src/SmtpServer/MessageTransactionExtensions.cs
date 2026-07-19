using System;
using System.Collections.Generic;

namespace SmtpServer
{
    /// <summary>
    /// Extension methods for message transactions.
    /// </summary>
    public static class MessageTransactionExtensions
    {
        /// <summary>
        /// Gets recipient-specific parameters when the transaction provides them.
        /// </summary>
        /// <param name="transaction">The message transaction.</param>
        /// <returns>The accepted recipients and their RCPT command parameters.</returns>
        public static IReadOnlyList<IMessageRecipient> GetRecipients(this IMessageTransaction transaction)
        {
            if (transaction is IParameterizedMessageTransaction parameterized)
            {
                return parameterized.Recipients;
            }

            return Array.Empty<IMessageRecipient>();
        }
    }
}

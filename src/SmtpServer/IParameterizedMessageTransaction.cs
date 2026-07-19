using System.Collections.Generic;

namespace SmtpServer
{
    /// <summary>
    /// Optional message transaction interface for recipient-specific parameters.
    /// </summary>
    public interface IParameterizedMessageTransaction
    {
        /// <summary>
        /// Gets the accepted recipients and their RCPT command parameters.
        /// </summary>
        IReadOnlyList<IMessageRecipient> Recipients { get; }
    }
}

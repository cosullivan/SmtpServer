using System;

namespace SmtpServer.Protocol
{
    /// <summary>
    /// Exception thrown when a message exceeds the maximum allowed size as defined by the SMTP SIZE extension (RFC 1870).
    /// </summary>
    public sealed class MaxMessageSizeExceededException : Exception
    {

    }
}

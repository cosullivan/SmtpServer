namespace SmtpServer.Protocol
{
    /// <summary>
    /// Smtp Response
    /// </summary>
    public class SmtpResponse
    {
        /// <summary>
        /// 250 Ok
        /// </summary>
        public static readonly SmtpResponse Ok = new SmtpResponse(SmtpReplyCode.Ok, "Ok");

        /// <summary>
        /// 220 ServiceReady
        /// </summary>
        public static readonly SmtpResponse ServiceReady = new SmtpResponse(SmtpReplyCode.ServiceReady, "ready when you are");

        /// <summary>
        /// 550 MailboxUnavailable
        /// </summary>
        public static readonly SmtpResponse MailboxUnavailable = new SmtpResponse(SmtpReplyCode.MailboxUnavailable, "mailbox unavailable");

        /// <summary>
        /// 553 MailboxNameNotAllowed
        /// </summary>
        public static readonly SmtpResponse MailboxNameNotAllowed = new SmtpResponse(SmtpReplyCode.MailboxNameNotAllowed, "mailbox name not allowed");

        /// <summary>
        /// 221 ServiceClosingTransmissionChannel
        /// </summary>
        public static readonly SmtpResponse ServiceClosingTransmissionChannel = new SmtpResponse(SmtpReplyCode.ServiceClosingTransmissionChannel, "bye");

        /// <summary>
        /// 501 SyntaxError
        /// </summary>
        public static readonly SmtpResponse SyntaxError = new SmtpResponse(SmtpReplyCode.SyntaxError, "syntax error");

        /// <summary>
        /// 552 SizeLimitExceeded
        /// </summary>
        public static readonly SmtpResponse SizeLimitExceeded = new SmtpResponse(SmtpReplyCode.SizeLimitExceeded, "size limit exceeded");

        /// <summary>
        /// 554 TransactionFailed
        /// </summary>
        public static readonly SmtpResponse NoValidRecipientsGiven = new SmtpResponse(SmtpReplyCode.TransactionFailed, "no valid recipients given");

        /// <summary>
        /// 535 AuthenticationFailed
        /// </summary>
        public static readonly SmtpResponse AuthenticationFailed = new SmtpResponse(SmtpReplyCode.AuthenticationFailed, "authentication failed");

        /// <summary>
        /// 235 AuthenticationSuccessful
        /// </summary>
        public static readonly SmtpResponse AuthenticationSuccessful = new SmtpResponse(SmtpReplyCode.AuthenticationSuccessful, "go ahead");

        /// <summary>
        /// 554 TransactionFailed
        /// </summary>
        public static readonly SmtpResponse TransactionFailed = new SmtpResponse(SmtpReplyCode.TransactionFailed);

        /// <summary>
        /// 503 BadSequence
        /// </summary>
        public static readonly SmtpResponse BadSequence = new SmtpResponse(SmtpReplyCode.BadSequence, "bad sequence of commands");

        /// <summary>
        /// 530 AuthenticationRequired
        /// </summary>
        public static readonly SmtpResponse AuthenticationRequired = new SmtpResponse(SmtpReplyCode.AuthenticationRequired, "authentication required");

        /// <summary>
        /// 552 MaxMessageSizeExceeded
        /// </summary>
        public static readonly SmtpResponse MaxMessageSizeExceeded = new SmtpResponse(SmtpReplyCode.SizeLimitExceeded, "message size exceeds fixed maximium message size");

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="replyCode">The reply code.</param>
        /// <param name="message">The reply message.</param>
        public SmtpResponse(SmtpReplyCode replyCode, string message = null)
        {
            ReplyCode = replyCode;
            Message = message;
        }

        /// <summary>
        /// Gets the Reply Code.
        /// </summary>
        public SmtpReplyCode ReplyCode { get; }

        /// <summary>
        /// Gets the response message.
        /// </summary>
        public string Message { get; }
    }
}

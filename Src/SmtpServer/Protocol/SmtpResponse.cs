namespace SmtpServer.Protocol
{
    public class SmtpResponse
    {
        public static readonly SmtpResponse Ok = new SmtpResponse(SmtpReplyCode.Ok, "Ok");
        public static readonly SmtpResponse ServiceReady = new SmtpResponse(SmtpReplyCode.ServiceReady, "ready when you are");
        public static readonly SmtpResponse MailboxUnavailable = new SmtpResponse(SmtpReplyCode.MailboxUnavailable, "mailbox unavailable");
        public static readonly SmtpResponse MailboxNameNotAllowed = new SmtpResponse(SmtpReplyCode.MailboxNameNotAllowed, "mailbox name not allowed");
        public static readonly SmtpResponse ServiceClosingTransmissionChannel = new SmtpResponse(SmtpReplyCode.ServiceClosingTransmissionChannel, "bye");
        public static readonly SmtpResponse SyntaxError = new SmtpResponse(SmtpReplyCode.SyntaxError, "syntax error");
        public static readonly SmtpResponse SizeLimitExceeded = new SmtpResponse(SmtpReplyCode.SizeLimitExceeded, "size limit exceeded");
        public static readonly SmtpResponse NoValidRecipientsGiven = new SmtpResponse(SmtpReplyCode.TransactionFailed, "no valid recipients given");
        public static readonly SmtpResponse AuthenticationFailed = new SmtpResponse(SmtpReplyCode.AuthenticationFailed, "authentication failed");
        public static readonly SmtpResponse AuthenticationSuccessful = new SmtpResponse(SmtpReplyCode.AuthenticationSuccessful, "go ahead");
        public static readonly SmtpResponse TransactionFailed = new SmtpResponse(SmtpReplyCode.TransactionFailed);
        public static readonly SmtpResponse BadSequence = new SmtpResponse(SmtpReplyCode.BadSequence, "bad sequence of commands");
        public static SmtpResponse AuthenticationRequired = new SmtpResponse(SmtpReplyCode.AuthenticationRequired, "authentication required");

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
        /// Gets the repsonse message.
        /// </summary>
        public string Message { get; }
    }
}
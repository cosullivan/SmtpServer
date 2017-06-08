namespace SmtpServer.Protocol
{
    public enum SmtpReplyCode
    {
        /// <summary>
        /// The service is ready.
        /// </summary>
        ServiceReady = 220,

        /// <summary>
        /// Goodbye.
        /// </summary>
        ServiceClosingTransmissionChannel = 221,
        
        /// <summary>
        /// Authentication was Successful.
        /// </summary>
        AuthenticationSuccessful = 235, 

        /// <summary>
        /// Everything was Ok.
        /// </summary>
        Ok = 250,

        /// <summary>
        /// Continue with the authentication.
        /// </summary>
        ContinueWithAuth = 334,

        /// <summary>
        /// Start the mail input.
        /// </summary>
        StartMailInput = 354,

        /// <summary>
        /// There is insufficent stored to handle the mail.
        /// </summary>
        InsufficientStorage = 452,

        /// <summary>
        /// The client is not permitted to connect.
        /// </summary>
        ClientNotPermitted = 454,

        /// <summary>
        /// Syntax error, command unrecognized (This may include errors such as command line too long).
        /// </summary>
        CommandUnrecognized = 500,

        /// <summary>
        /// Syntax error in parameters or arguments.
        /// </summary>
        SyntaxError = 501,

        /// <summary>
        /// The command has not been implemented.
        /// </summary>
        CommandNotImplemented = 502,

        /// <summary>
        /// Bad sequence of commands.
        /// </summary>
        BadSequence = 503,

        /// <summary>
        /// Authentication required
        /// </summary>
        AuthenticationRequired = 530,

        /// <summary>
        /// Authentication failed.
        /// </summary>
        AuthenticationFailed = 535,

        /// <summary>
        /// The Mailbox is temporarily unavailable.
        /// </summary>
        MailboxUnavailable = 550,

        /// <summary>
        /// The size limit has been exceeded.
        /// </summary>
        SizeLimitExceeded = 552,

        /// <summary>
        /// The Mailbox is permanently not available.
        /// </summary>
        MailboxNameNotAllowed = 553,

        /// <summary>
        /// The transaction failed.
        /// </summary>
        TransactionFailed = 554
    }
}
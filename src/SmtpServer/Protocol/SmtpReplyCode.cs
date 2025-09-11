namespace SmtpServer.Protocol
{
    /// <summary>
    /// Smtp Reply Code
    /// </summary>
    public enum SmtpReplyCode
    {
        /// <summary>
        /// The server is unable to connect.
        /// </summary>
        UnableToConnect = 101,

        /// <summary>
        /// Connection refused or inability to open an SMTP stream.
        /// </summary>
        ConnectionRefused = 111,

        /// <summary>
        /// System status message or help reply.
        /// </summary>
        SystemMessage = 211,

        /// <summary>
        /// A response to the HELP command.
        /// </summary>
        HelpResponse = 214,

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
        /// "User not local will forward": the recipient’s account is not on the present server, so it will be relayed to another.
        /// </summary>
        RelayToAnotherServer = 251,

        /// <summary>
        /// The server cannot verify the user, but it will try to deliver the message anyway.
        /// </summary>
        CantVerifyUser = 252,

        /// <summary>
        /// Continue with the authentication.
        /// </summary>
        ContinueWithAuth = 334,

        /// <summary>
        /// Start the mail input.
        /// </summary>
        StartMailInput = 354,

        /// <summary>
        /// "Timeout connection problem": there have been issues during the message transfer.
        /// </summary>
        TimeoutConnectionProblem = 420,

        /// <summary>
        /// The service is unavailable due to a connection problem: it may refer to an exceeded limit of simultaneous connections, or a more general temporary problem.
        /// The server (yours or the recipient's) is not available at the moment, so the dispatch will be tried again later.
        /// </summary>
        ServiceUnavailable = 421,

        /// <summary>
        /// The recipient’s mailbox has exceeded its storage limit.
        /// </summary>
        ExceededStorage = 422,

        /// <summary>
        /// Not enough space on the disk, or an "out of memory" condition due to a file overload.
        /// </summary>
        Overloaded = 431,

        /// <summary>
        /// The recipient’s server is not responding.
        /// </summary>
        RecipientNotResponding = 441,

        /// <summary>
        /// The connection was dropped during the transmission.
        /// </summary>
        ConnectionDropped = 442,

        /// <summary>
        /// The maximum hop count was exceeded for the message: an internal loop has occurred.
        /// </summary>
        MaxHopCountExceeded = 446,

        /// <summary>
        /// Your outgoing message timed out because of issues concerning the incoming server.
        /// </summary>
        MessageTimeout = 447,

        /// <summary>
        /// A routing error.
        /// </summary>
        RoutingError = 449,

        /// <summary>
        /// "Requested action not taken – The user’s mailbox is unavailable". The mailbox has been corrupted or placed on an offline server, or your email hasn't been accepted for IP problems or blacklisting.
        /// </summary>
        Unavailable = 450,

        /// <summary>
        /// "Requested action aborted – Local error in processing". Your ISP's server or the server that got a first relay from yours has encountered a connection problem.
        /// </summary>
        Aborted = 451,

        /// <summary>
        /// There is insufficent stored to handle the mail.
        /// </summary>
        InsufficientStorage = 452,

        /// <summary>
        /// The client is not permitted to connect.
        /// </summary>
        ClientNotPermitted = 454,

        /// <summary>
        /// An error of your mail server, often due to an issue of the local anti-spam filter.
        /// </summary>
        Error = 471,

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
        /// A command parameter is not implemented.
        /// </summary>
        CommandParameterNotImplemented = 504,

        /// <summary>
        /// Bad email address.
        /// Codes 510 or 511 result the same structure.
        /// One of the addresses in your TO, CC or BBC line doesn't exist. Check again your recipients' accounts and correct any possible misspelling.
        /// </summary>
        BadEmailAddress = 510,

        /// <summary>
        /// A DNS error: the host server for the recipient's domain name cannot be found.
        /// </summary>
        DnsError = 512,

        /// <summary>
        /// "Address type is incorrect": another problem concerning address misspelling. In few cases, however, it's related to an authentication issue.
        /// </summary>
        IncorrectAddressType = 513,

        /// <summary>
        /// The total size of your mailing exceeds the recipient server's limits.
        /// </summary>
        MailingLimitExceeded = 523,

        /// <summary>
        /// Authentication required
        /// </summary>
        AuthenticationRequired = 530,

        /// <summary>
        /// Authentication failed.
        /// </summary>
        AuthenticationFailed = 535,

        /// <summary>
        /// The recipient address rejected your message: normally, it's an error caused by an anti-spam filter.
        /// </summary>
        RecipientAddressRejected = 541,

        /// <summary>
        /// The Mailbox is temporarily unavailable.
        /// </summary>
        MailboxUnavailable = 550,

        /// <summary>
        /// "User not local or invalid address – Relay denied". Meaning, if both your address and the recipients are not locally hosted by the server, a relay can be interrupted.
        /// </summary>
        RelayDenied = 551,

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

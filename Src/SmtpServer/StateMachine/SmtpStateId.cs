namespace SmtpServer.StateMachine
{
    internal enum SmtpStateId
    {
        None = 0,
        Initialized = 1,
        WaitingForMail = 2,
        WaitingForMailSecure = 3,
        WithinTransaction = 4,
        CanAcceptData = 5,
    }
}

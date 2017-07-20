namespace SmtpServer.Protocol
{
    internal enum SmtpState
    {
        None = 0,
        Initialized = 1,
        WaitingForMail = 2,
        WaitingForMailSecure = 3,
        WithinTransaction = 4,
        CanAcceptData = 5,
    }
}
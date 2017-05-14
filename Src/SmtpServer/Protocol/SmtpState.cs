namespace SmtpServer.Protocol
{
    internal enum SmtpState
    {
        Initialized = 0,
        WaitingForMail = 1,
        WaitingForMailSecure = 2,
        WithinTransaction = 3,
        CanAcceptData = 4,
    }
}
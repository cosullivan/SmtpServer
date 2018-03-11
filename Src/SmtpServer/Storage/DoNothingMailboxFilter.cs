namespace SmtpServer.Storage
{
    internal sealed class DoNothingMailboxFilter : MailboxFilter
    {
        internal static readonly DoNothingMailboxFilter Instance = new DoNothingMailboxFilter();
    }
}
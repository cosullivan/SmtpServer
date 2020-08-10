namespace SmtpServer.Storage
{
    internal sealed class DoNothingMailboxFilter : MailboxFilter
    {
        internal static readonly IMailboxFilterFactory Default = new DoNothingMailboxFilter();
    }
}
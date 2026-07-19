using System.Collections.Generic;
using SmtpServer.Mail;

namespace SmtpServer
{
    sealed class SmtpMessageRecipient : IMessageRecipient
    {
        public SmtpMessageRecipient(IMailbox address, IReadOnlyDictionary<string, string> parameters)
        {
            Address = address;
            Parameters = parameters ?? new Dictionary<string, string>();
        }

        public IMailbox Address { get; }

        public IReadOnlyDictionary<string, string> Parameters { get; }
    }
}

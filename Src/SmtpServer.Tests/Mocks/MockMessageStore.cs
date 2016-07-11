using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SmtpServer.Mail;
using SmtpServer.Protocol;
using SmtpServer.Storage;

namespace SmtpServer.Tests.Mocks
{
    public class MockMessageStore : MessageStore
    {
        readonly List<IMimeMessage> _messages = new List<IMimeMessage>();

        public override Task<SmtpResponse> SaveAsync(ISessionContext context, IMimeMessage message, CancellationToken cancellationToken)
        {
            _messages.Add(message);

            return Task.FromResult(SmtpResponse.Ok);
        }

        public List<IMimeMessage> Messages
        {
            get { return _messages; }
        }
    }
}

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SmtpServer.Protocol;
using SmtpServer.Storage;

namespace SmtpServer.Tests.Mocks
{
    public class MockMessageStore : MessageStore
    {
        readonly List<IMessageTransaction> _messages = new List<IMessageTransaction>();

        public override Task<SmtpResponse> SaveAsync(ISessionContext context, IMessageTransaction transaction, CancellationToken cancellationToken)
        {
            _messages.Add(transaction);

            return Task.FromResult(SmtpResponse.Ok);
        }

        public List<IMessageTransaction> Messages
        {
            get { return _messages; }
        }
    }
}
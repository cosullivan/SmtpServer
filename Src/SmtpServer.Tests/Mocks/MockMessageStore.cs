using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using SmtpServer.Mail;
using SmtpServer.Storage;

namespace SmtpServer.Tests.Mocks
{
    public class MockMessageStore : IMessageStore
    {
        readonly List<IMimeMessage> _messages = new List<IMimeMessage>();

        public Task<string> SaveAsync(IMimeMessage message, CancellationToken cancellationToken)
        {
            _messages.Add(message);

            return Task.FromResult(_messages.Count.ToString(CultureInfo.InvariantCulture));
        }

        public List<IMimeMessage> Messages
        {
            get { return _messages; }
        }
    }
}

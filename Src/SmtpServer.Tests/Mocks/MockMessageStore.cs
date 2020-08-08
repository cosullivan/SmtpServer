using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using MimeKit;
using SmtpServer.Protocol;
using SmtpServer.Storage;

namespace SmtpServer.Tests.Mocks
{
    public sealed class MockMessageStore : MessageStore
    {
        public MockMessageStore()
        {
            Messages = new List<MockMessage>();
        }

        public override Task<SmtpResponse> SaveAsync(ISessionContext context, IMessageTransaction transaction, ReadOnlySequence<byte> buffer, CancellationToken cancellationToken)
        {
            Messages.Add(new MockMessage(transaction, buffer));;

            return Task.FromResult(SmtpResponse.Ok);
        }

        public List<MockMessage> Messages { get; }
    }

    public sealed class MockMessage
    {
        public MockMessage(IMessageTransaction transaction, ReadOnlySequence<byte> buffer)
        {
            Transaction = transaction;
            
            using var stream = new MemoryStream(buffer.ToArray());

            MimeMessage = MimeMessage.Load(stream);
        }

        public string Text(string charset = "utf-8")
        {
            return ((TextPart)MimeMessage.Body).GetText(charset).TrimEnd('\n', '\r');
        }

        public IMessageTransaction Transaction { get; }

        public MimeMessage MimeMessage { get; }
    }
}
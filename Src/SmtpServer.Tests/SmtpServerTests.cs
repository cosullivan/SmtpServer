using System;
using System.Net.Mail;
using System.Threading;
using System.Threading.Tasks;
using SmtpServer.Mail;
using SmtpServer.Tests.Mocks;
using Xunit;

namespace SmtpServer.Tests
{
    public class SmtpServerTests
    {
        readonly MockMessageStore _messageStore;
        readonly OptionsBuilder _optionsBuilder;

        public SmtpServerTests()
        {
            _messageStore = new MockMessageStore();

            _optionsBuilder = new OptionsBuilder()
                .ServerName("localhost")
                .Port(25)
                .MessageStore(_messageStore);
        }

        [Fact]
        public void CanReceiveMessage()
        {
            // arrange
            var smtpServer = new SmtpServer(_optionsBuilder.Build());
            var smtpServerTask = smtpServer.StartAsync(CancellationToken.None);
            using (var smtpClient = new SmtpClient("localhost", 25))
            {

                // act
                smtpClient.Send("test1@test.com", "test2@test.com", "Test", "Test Message");

                // assert
                Assert.Equal(1, _messageStore.Messages.Count);
                Assert.Equal("test1@test.com", _messageStore.Messages[0].From.AsAddress());
                Assert.Equal(1, _messageStore.Messages[0].To.Count);
                Assert.Equal("test2@test.com", _messageStore.Messages[0].To[0].AsAddress());
            }
        }
    }
}

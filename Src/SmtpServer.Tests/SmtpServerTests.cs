using System;
using System.Threading;
using System.Threading.Tasks;
using SmtpServer.Mail;
using SmtpServer.Tests.Mocks;
using Xunit;
using MailKit.Net.Smtp;
using MimeKit;

namespace SmtpServer.Tests
{
    public class SmtpServerTests
    {
        readonly MockMessageStore _messageStore;
        readonly OptionsBuilder _optionsBuilder;
        readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

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
            var smtpServerTask = smtpServer.StartAsync(_cancellationTokenSource.Token);
            var mimeMessage = new MimeKit.MimeMessage();
            mimeMessage.From.Add(new MailboxAddress("Test", "test1@test.com"));
            mimeMessage.To.Add(new MailboxAddress("Destinatary", "test2@test.com"));
            mimeMessage.Subject = "Test";
            mimeMessage.Body = new TextPart("plain")
            {
                Text = "Test message to server"
            };

            // act
            using (var client = new SmtpClient())
            {
                client.Connect("localhost", 25);
                client.Send(mimeMessage);
                client.Disconnect(true);
            }

            // assert
            Assert.Equal(1, _messageStore.Messages.Count);
            Assert.Equal("test1@test.com", _messageStore.Messages[0].From.AsAddress());
            Assert.Equal(1, _messageStore.Messages[0].To.Count);
            Assert.Equal("test2@test.com", _messageStore.Messages[0].To[0].AsAddress());

            Wait(smtpServerTask);
        }

        void Wait(Task smtpServerTask)
        {
            try
            {
                _cancellationTokenSource.Cancel();
                smtpServerTask.Wait();
            }
            catch (AggregateException e)
            {
                e.Handle(exception => exception is OperationCanceledException);
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }
        }
    }
}

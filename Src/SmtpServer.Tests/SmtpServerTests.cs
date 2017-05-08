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
            var smtpClient = new SmtpClient();
            var smtpServerTask = smtpServer.StartAsync(_cancellationTokenSource.Token);

            var message = new MimeKit.MimeMessage();
            message.From.Add(new MailboxAddress("test1@test.com"));
            message.To.Add(new MailboxAddress("test2@test.com"));
            message.Subject = "Test";
            message.Body = new TextPart("plain")
            {
                Text = "Test Message"
            };

            // act
            smtpClient.Connect("localhost", 25, false);
            smtpClient.Send(message);

            // assert
            Assert.Equal(1, _messageStore.Messages.Count);
            Assert.Equal("test1@test.com", _messageStore.Messages[0].From.AsAddress());
            Assert.Equal(1, _messageStore.Messages[0].To.Count);
            Assert.Equal("test2@test.com", _messageStore.Messages[0].To[0].AsAddress());

            Wait(smtpServerTask);
        }

        [Fact]
        public void CanReceive8BitMimeMessage()
        {
            // arrange
            var smtpServer = new SmtpServer(_optionsBuilder.Build());
            var smtpClient = new SmtpClient();
            var smtpServerTask = smtpServer.StartAsync(_cancellationTokenSource.Token);

            var message = new MimeKit.MimeMessage();
            message.From.Add(new MailboxAddress("test1@test.com"));
            message.To.Add(new MailboxAddress("test2@test.com"));
            message.Subject = "Assunto teste acento çãõáéíóú";
            message.Body = new TextPart("plain")
            {
                Text = "Assunto teste acento çãõáéíóú"
            };
            
            // act
            smtpClient.Connect("localhost", 25, false);
            smtpClient.Send(message);

            // assert
            Assert.Equal(1, _messageStore.Messages.Count);

            Wait(smtpServerTask);
        }

        void Wait(Task smtpServerTask)
        {
            _cancellationTokenSource.Cancel();

            try
            {
                smtpServerTask.Wait();
            }
            catch (AggregateException e)
            {
                e.Handle(exception => exception is OperationCanceledException);
            }
        }
    }
}
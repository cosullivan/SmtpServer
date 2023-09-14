using SmtpServer;
using SmtpServer.ComponentModel;
using SmtpServer.Tests.Mocks;

namespace SampleTests
{
    [TestClass]
    public class SendingEmailTests
    {
        MockMessageStore TestMessageStore = new MockMessageStore();

        [TestMethod]
        public void SendEmail()
        {
            // Arrange
            var cancellationTokenSource = new CancellationTokenSource();

            var options = new SmtpServerOptionsBuilder()
                .ServerName("SmtpServer SampleApp")
                .Port(25)
            .Build();

            var serviceProvider = new ServiceProvider();
            serviceProvider.Add(TestMessageStore);

            var server = new SmtpServer.SmtpServer(options, serviceProvider);
            var serverTask = server.StartAsync(cancellationTokenSource.Token);

            // Act
            SampleMailClient.Send(port: 25);

            // Assert
            cancellationTokenSource.Cancel();
            try
            {
                serverTask.Wait();
            }
            catch (AggregateException e)
            {
                e.Handle(exception => exception is OperationCanceledException);
            }

            Assert.AreEqual(1, this.TestMessageStore.Messages.Count);
            var message = this.TestMessageStore.Messages.First();
            Assert.AreEqual(1, message.MimeMessage.From.Count);
            var fromAddress = message.MimeMessage.From.Mailboxes.Single();
            Assert.AreEqual("", fromAddress.Name);
            Assert.AreEqual("from@sample.com", fromAddress.Address);
        }
    }
}
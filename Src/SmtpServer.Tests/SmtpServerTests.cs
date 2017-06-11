using System;
using System.Threading;
using SmtpServer.Mail;
using SmtpServer.Tests.Mocks;
using Xunit;
using SmtpServer.Authentication;

namespace SmtpServer.Tests
{
    public class SmtpServerTests
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        public SmtpServerTests()
        {
            MessageStore = new MockMessageStore();
            CancellationTokenSource = new CancellationTokenSource();
        }

        [Fact]
        public void CanReceiveMessage()
        {
            using (CreateServer())
            {
                // act
                MailClient.Send(from: "test1@test.com", to: "test2@test.com");

                // assert
                Assert.Equal(1, MessageStore.Messages.Count);
                Assert.Equal("test1@test.com", MessageStore.Messages[0].From.AsAddress());
                Assert.Equal(1, MessageStore.Messages[0].To.Count);
                Assert.Equal("test2@test.com", MessageStore.Messages[0].To[0].AsAddress());
            }
        }

        [Fact]
        public void CanReceive8BitMimeMessage()
        {
            using (CreateServer())
            {
                // act
                MailClient.Send(subject: "Assunto teste acento çãõáéíóú");

                // assert
                Assert.Equal(1, MessageStore.Messages.Count);
                Assert.Equal("Assunto teste acento çãõáéíóú", MessageStore.Messages[0].Subject());
            }
        }

        [Fact]
        public void CanAuthenticateUser()
        {
            // arrange
            string user = null;
            string password = null;
            var userAuthenticator = new DelegatingUserAuthenticator((u, p) =>
            {
                user = u;
                password = p;

                return true;
            });

            using (CreateServer(options => options.AllowUnsecureAuthentication().UserAuthenticator(userAuthenticator)))
            {
                // act
                MailClient.Send(user: "user", password: "password");

                // assert
                Assert.Equal(1, MessageStore.Messages.Count);
                Assert.Equal("user", user);
                Assert.Equal("password", password);
            }
        }

        [Fact]
        public void CanReceiveBccInMessageTransaction()
        {
            using (CreateServer())
            {
                // act
                MailClient.Send(from: "test1@test.com", to: "test2@test.com", cc: "test3@test.com", bcc: "test4@test.com");

                // assert
                Assert.Equal(1, MessageStore.Messages.Count);
                Assert.Equal("test1@test.com", MessageStore.Messages[0].From.AsAddress());
                Assert.Equal(3, MessageStore.Messages[0].To.Count);
                Assert.Equal("test2@test.com", MessageStore.Messages[0].To[0].AsAddress());
                Assert.Equal("test3@test.com", MessageStore.Messages[0].To[1].AsAddress());
                Assert.Equal("test4@test.com", MessageStore.Messages[0].To[2].AsAddress());
            }
        }

        /// <summary>
        /// Create a running instance of a server.
        /// </summary>
        /// <returns>A disposable instance which will close and release the server instance.</returns>
        IDisposable CreateServer()
        {
            return CreateServer(options => { });
        }

        /// <summary>
        /// Create a running instance of a server.
        /// </summary>
        /// <param name="configuration">The configuration to apply to run the server.</param>
        /// <returns>A disposable instance which will close and release the server instance.</returns>
        IDisposable CreateServer(Action<OptionsBuilder> configuration)
        {
            var options = new OptionsBuilder()
                .ServerName("localhost")
                .Port(9025)
                .MessageStore(MessageStore);

            configuration(options);

            var smtpServerTask = new SmtpServer(options.Build()).StartAsync(CancellationTokenSource.Token);

            return new DelegatingDisposable(() =>
            {
                CancellationTokenSource.Cancel();

                try
                {
                    smtpServerTask.Wait();
                }
                catch (AggregateException e)
                {
                    e.Handle(exception => exception is OperationCanceledException);
                }
            });
        }

        /// <summary>
        /// The message store that is being used to store the messages by default.
        /// </summary>
        public MockMessageStore MessageStore { get; }

        /// <summary>
        /// The cancellation token source for the test.
        /// </summary>
        public CancellationTokenSource CancellationTokenSource { get; }
    }

    internal static class MessageTransactionExtensions
    {
        /// <summary>
        /// Returns the subject from the message.
        /// </summary>
        /// <param name="messageTransaction">The message transaction to return the message subject from.</param>
        /// <returns>The message subject from the message transaction.</returns>
        public static string Subject(this IMessageTransaction messageTransaction)
        {
            if (messageTransaction == null)
            {
                throw new ArgumentNullException(nameof(messageTransaction));
            }

            var textMessage = (ITextMessage)messageTransaction.Message;

            return MimeKit.MimeMessage.Load(textMessage.Content).Subject;
        }

        /// <summary>
        /// Returns the text message body.
        /// </summary>
        /// <param name="messageTransaction">The message transaction to return the message text body from.</param>
        /// <returns>The message text body from the message transaction.</returns>
        public static string Text(this IMessageTransaction messageTransaction)
        {
            if (messageTransaction == null)
            {
                throw new ArgumentNullException(nameof(messageTransaction));
            }

            var textMessage = (ITextMessage) messageTransaction.Message;

            return MimeKit.MimeMessage.Load(textMessage.Content).TextBody;
        }
    }
}
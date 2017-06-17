using System;
using System.Threading;
using MailKit;
using SmtpServer.Mail;
using SmtpServer.Tests.Mocks;
using Xunit;
using SmtpServer.Authentication;
using SmtpServer.Protocol;
using SmtpServer.Storage;

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

        [Theory]
        [InlineData("Assunto teste acento çãõáéíóú", "utf-8")]
        [InlineData("שלום שלום שלום", "windows-1255")]
        public void CanReceiveUnicodeMimeMessage(string text, string charset)
        {
            using (CreateServer())
            {
                // act
                MailClient.Send(subject: text, text: text, charset: charset);

                // assert
                Assert.Equal(1, MessageStore.Messages.Count);
                Assert.Equal(text, MessageStore.Messages[0].Subject());
                Assert.Equal(text, MessageStore.Messages[0].Text(charset));
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

        [Fact]
        public void CanReturnSmtpResponseException()
        {
            // arrange
            var mailboxFilter = new DelegatingMailboxFilter(@from =>
            {
                throw new SmtpResponseException(SmtpResponse.AuthenticationRequired);

#pragma warning disable 162
                return MailboxFilterResult.Yes;
#pragma warning restore 162
            });

            using (CreateServer(options => options.MailboxFilter(mailboxFilter)))
            {
                Assert.Throws<ServiceNotAuthenticatedException>(() => MailClient.Send());
            }
        }

        [Fact]
        public void CanForceUserAuthentication_DoesNotThrowIfLoginIsSent()
        {
            var userAuthenticator = new DelegatingUserAuthenticator((user, password) => true);

            using (CreateServer(options => options.AllowUnsecureAuthentication().AuthenticationRequired().UserAuthenticator(userAuthenticator)))
            {
                MailClient.Send(user: "user", password: "password");
            }
        }

        [Fact]
        public void CanForceUserAuthentication_ThrowsIfLoginIsNotSent()
        {
            var userAuthenticator = new DelegatingUserAuthenticator((user, password) => true);

            using (CreateServer(options => options.AllowUnsecureAuthentication().AuthenticationRequired().UserAuthenticator(userAuthenticator)))
            {
                Assert.Throws<ServiceNotAuthenticatedException>(() => MailClient.Send());
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
}
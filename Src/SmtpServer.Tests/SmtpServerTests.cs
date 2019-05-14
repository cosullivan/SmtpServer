using System;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using MailKit;
using MailKit.Net.Smtp;
using SmtpServer.Mail;
using SmtpServer.Tests.Mocks;
using Xunit;
using SmtpServer.Authentication;
using SmtpServer.Net;
using SmtpServer.Protocol;
using SmtpServer.Storage;
using SmtpResponse = SmtpServer.Protocol.SmtpResponse;

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
                MailClient.Send(MailClient.Message(from: "test1@test.com", to: "test2@test.com"));

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
                MailClient.Send(MailClient.Message(subject: text, text: text, charset: charset));

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

            using (CreateServer(server => server.UserAuthenticator(userAuthenticator), endpoint => endpoint.AllowUnsecureAuthentication()))
            {
                // act
                MailClient.Send(user: "user", password: "password");

                // assert
                Assert.Equal(1, MessageStore.Messages.Count);
                Assert.Equal("user", user);
                Assert.Equal("password", password);
            }
        }

        [Theory]
        [InlineData("", "")]
        [InlineData("user", "")]
        [InlineData("", "password")]
        public void CanFailAuthenticationEmptyUserOrPassword(string user, string password)
        {
            // arrange
            string actualUser = null;
            string actualPassword = null;
            var userAuthenticator = new DelegatingUserAuthenticator((u, p) =>
            {
                actualUser = u;
                actualPassword = p;

                return false;
            });

            using (CreateServer(server => server.UserAuthenticator(userAuthenticator), endpoint => endpoint.AllowUnsecureAuthentication()))
            {
                // act and assert
                Assert.Throws<MailKit.Security.AuthenticationException>(() => MailClient.Send(user: user, password: password));

                // assert
                Assert.Equal(0, MessageStore.Messages.Count);
                Assert.Equal(user, actualUser);
                Assert.Equal(password, actualPassword);
            }
        }

        [Fact]
        public void CanReceiveBccInMessageTransaction()
        {
            using (CreateServer())
            {
                // act
                MailClient.Send(MailClient.Message(from: "test1@test.com", to: "test2@test.com", cc: "test3@test.com", bcc: "test4@test.com"));

                // assert
                Assert.Equal(1, MessageStore.Messages.Count);
                Assert.Equal("test1@test.com", MessageStore.Messages[0].From.AsAddress());
                Assert.Equal(3, MessageStore.Messages[0].To.Count);
                Assert.Equal("test2@test.com", MessageStore.Messages[0].To[0].AsAddress());
                Assert.Equal("test3@test.com", MessageStore.Messages[0].To[1].AsAddress());
                Assert.Equal("test4@test.com", MessageStore.Messages[0].To[2].AsAddress());
            }
        }

        [Fact(Skip = "Command timeout wont work properly until https://github.com/dotnet/corefx/issues/15033")]
        public void WillTimeoutWaitingForCommand()
        {
            using (CreateServer(c => c.CommandWaitTimeout(TimeSpan.FromSeconds(1))))
            {
                var client = MailClient.Client();
                client.NoOp();

                for (var i = 0; i < 5; i++)
                {
                    Task.Delay(TimeSpan.FromMilliseconds(250)).Wait();
                    client.NoOp();
                }

                Task.Delay(TimeSpan.FromSeconds(5)).Wait();

                Assert.Throws<IOException>(() => client.NoOp());
            }
        }

        [Fact]
        public void CanReturnSmtpResponseException_DoesNotQuit()
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
                using (var client = MailClient.Client())
                {
                    Assert.Throws<ServiceNotAuthenticatedException>(() => client.Send(MailClient.Message()));

                    client.NoOp();
                }
            }
        }

        [Fact]
        public void CanReturnSmtpResponseException_SessionWillQuit()
        {
            // arrange
            var mailboxFilter = new DelegatingMailboxFilter(@from => throw new SmtpResponseException(SmtpResponse.AuthenticationRequired, true));

            using (CreateServer(options => options.MailboxFilter(mailboxFilter)))
            {
                using (var client = MailClient.Client())
                {
                    Assert.Throws<SmtpProtocolException>(() => client.Send(MailClient.Message()));

                    // no longer connected to this is invalid
                    Assert.ThrowsAny<Exception>(() => client.NoOp());
                }
            }
        }

        [Fact]
        public void CanForceUserAuthentication_DoesNotThrowIfLoginIsSent()
        {
            var userAuthenticator = new DelegatingUserAuthenticator((user, password) => true);

            using (CreateServer(server => server.UserAuthenticator(userAuthenticator), endpoint => endpoint.AllowUnsecureAuthentication().AuthenticationRequired()))
            {
                MailClient.Send(user: "user", password: "password");
            }
        }

        [Fact]
        public void CanForceUserAuthentication_ThrowsIfLoginIsNotSent()
        {
            var userAuthenticator = new DelegatingUserAuthenticator((user, password) => true);

            using (CreateServer(server => server.UserAuthenticator(userAuthenticator), endpoint => endpoint.AllowUnsecureAuthentication().AuthenticationRequired()))
            {
                Assert.Throws<ServiceNotAuthenticatedException>(() => MailClient.Send());
            }
        }

        [Fact]
        public void DoesNotSecureTheSessionWhenCertificateIsEmpty()
        {
            using (var disposable = CreateServer())
            {
                ISessionContext sessionContext = null;
                var sessionCreatedHandler = new EventHandler<SessionEventArgs>(
                    delegate (object sender, SessionEventArgs args)
                    {
                        sessionContext = args.Context;
                    });

                disposable.Server.SessionCreated += sessionCreatedHandler;

                MailClient.Send();

                disposable.Server.SessionCreated -= sessionCreatedHandler;

                Assert.False(sessionContext.NetworkClient.IsSecure);
            }

            ServicePointManager.ServerCertificateValidationCallback = null;
        }

        [Fact]
        public void SecuresTheSessionWhenCertificateIsSupplied()
        {
            ServicePointManager.ServerCertificateValidationCallback = IgnoreCertificateValidationFailureForTestingOnly;

            using (var disposable = CreateServer(options => options.Certificate(CreateCertificate())))
            {
                ISessionContext sessionContext = null;
                var sessionCreatedHandler = new EventHandler<SessionEventArgs>(
                    delegate(object sender, SessionEventArgs args)
                    {
                        sessionContext = args.Context;
                    });

                disposable.Server.SessionCreated += sessionCreatedHandler;

                MailClient.Send();

                disposable.Server.SessionCreated -= sessionCreatedHandler;

                Assert.True(sessionContext.NetworkClient.IsSecure);
            }

            ServicePointManager.ServerCertificateValidationCallback = null;
        }

        [Fact]
        public void ServerCanBeSecuredAndAuthenticated()
        {
            var userAuthenticator = new DelegatingUserAuthenticator((user, password) => true);

            ServicePointManager.ServerCertificateValidationCallback = IgnoreCertificateValidationFailureForTestingOnly;

            using (var disposable = CreateServer(
                server => 
                    server
                        .UserAuthenticator(userAuthenticator)
                        .Certificate(CreateCertificate())
                        .SupportedSslProtocols(SslProtocols.Tls12),
                endpoint => 
                    endpoint.AllowUnsecureAuthentication(true)))
            {
                ISessionContext sessionContext = null;
                var sessionCreatedHandler = new EventHandler<SessionEventArgs>(
                    delegate (object sender, SessionEventArgs args)
                    {
                        sessionContext = args.Context;
                    });

                disposable.Server.SessionCreated += sessionCreatedHandler;

                MailClient.Send(user: "user", password: "password");

                disposable.Server.SessionCreated -= sessionCreatedHandler;

                Assert.True(sessionContext.NetworkClient.IsSecure);
                Assert.True(sessionContext.IsAuthenticated);
            }

            ServicePointManager.ServerCertificateValidationCallback = null;
        }

        [Fact]
        public void EndpointListenerWillRaiseEndPointEvents()
        {
            var endpointListenerFactory = new EndpointListenerFactory();

            var started = false;
            var stopped = false;

            endpointListenerFactory.EndpointStarted += (sender, e) => { started = true; };
            endpointListenerFactory.EndpointStopped += (sender, e) => { stopped = true; };

            using (CreateServer(server => server.EndpointListenerFactory(endpointListenerFactory)))
            {
                MailClient.Send();
            }

            Assert.True(started);
            Assert.True(stopped);
        }

        public static bool IgnoreCertificateValidationFailureForTestingOnly(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }

        public static X509Certificate2 CreateCertificate()
        {
            var certificate = File.ReadAllBytes(@"C:\Dropbox\Documents\Cain\Programming\SmtpServer\SmtpServer.pfx");
            var password = File.ReadAllText(@"C:\Dropbox\Documents\Cain\Programming\SmtpServer\SmtpServerPassword.txt");

            return new X509Certificate2(certificate, password);
        }

        /// <summary>
        /// Create a running instance of a server.
        /// </summary>
        /// <returns>A disposable instance which will close and release the server instance.</returns>
        SmtpServerDisposable CreateServer()
        {
            return CreateServer(options => { }, options => { });
        }

        /// <summary>
        /// Create a running instance of a server.
        /// </summary>
        /// <param name="serverConfiguration">The configuration to apply to run the server.</param>
        /// <returns>A disposable instance which will close and release the server instance.</returns>
        SmtpServerDisposable CreateServer(Action<SmtpServerOptionsBuilder> serverConfiguration)
        {
            return CreateServer(serverConfiguration, endpointConfiguration => { });
        }

        /// <summary>
        /// Create a running instance of a server.
        /// </summary>
        /// <param name="serverConfiguration">The configuration to apply to run the server.</param>
        /// <param name="endpointConfiguration">The configuration to apply to the endpoint.</param>
        /// <returns>A disposable instance which will close and release the server instance.</returns>
        SmtpServerDisposable CreateServer(
            Action<SmtpServerOptionsBuilder> serverConfiguration, 
            Action<EndpointDefinitionBuilder> endpointConfiguration)
        {
            var options = new SmtpServerOptionsBuilder()
                .ServerName("localhost")
                .Endpoint(
                    endpointBuilder =>
                    {
                        endpointBuilder.Port(9025);
                        endpointConfiguration(endpointBuilder);
                    })
                .MessageStore(MessageStore);

            serverConfiguration(options);

            var server = new SmtpServer(options.Build());
            var smtpServerTask = server.StartAsync(CancellationTokenSource.Token);

            return new SmtpServerDisposable(server, () =>
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
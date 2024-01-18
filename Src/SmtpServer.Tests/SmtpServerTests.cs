using System;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using MailKit;
using MailKit.Security;
using SmtpServer.Mail;
using SmtpServer.Tests.Mocks;
using Xunit;
using SmtpServer.Authentication;
using SmtpServer.ComponentModel;
using SmtpServer.Net;
using SmtpServer.Protocol;
using SmtpServer.Storage;
using SmtpResponse = SmtpServer.Protocol.SmtpResponse;
using System.Net.Sockets;

namespace SmtpServer.Tests
{
    public class SmtpServerTests
    {
        private const string Base64Certificate = "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";
        private const string CertificatePassword = "awesome";

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
                Assert.Single(MessageStore.Messages);
                Assert.Equal("test1@test.com", MessageStore.Messages[0].Transaction.From.AsAddress());
                Assert.Equal(1, MessageStore.Messages[0].Transaction.To.Count);
                Assert.Equal("test2@test.com", MessageStore.Messages[0].Transaction.To[0].AsAddress());
            }
        }

        [Theory]
        [InlineData("Assunto teste acento çãõáéíóú", "utf-8", true)]
        [InlineData("שלום שלום שלום", "windows-1255", false)]
        // Adjusted this test because "windows-1255" ending is not supported by .NET Core
        public void CanReceiveUnicodeMimeMessage(string text, string charset, bool shouldPass)
        {
            using (CreateServer())
            {
                Exception exceptionCaught = null;
                try
                {
                    // act
                    MailClient.Send(MailClient.Message(subject: text, text: text, charset: charset));
                }
                catch (Exception ex)
                {
                    exceptionCaught = ex;
                }

                if (shouldPass)
                {
                    // assert
                    Assert.Null(exceptionCaught);
                    Assert.Single(MessageStore.Messages);
                    Assert.Equal(text, MessageStore.Messages[0].MimeMessage.Subject);
                    Assert.Equal(text, MessageStore.Messages[0].Text(charset));
                }
                else
                {
                    //check if some exception was thrown
                    Assert.NotNull(exceptionCaught);
                }
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

            using (CreateServer(endpoint => endpoint.AllowUnsecureAuthentication(), services => services.Add(userAuthenticator)))
            {
                // act
                MailClient.Send(user: "user", password: "password");

                // assert
                Assert.Single(MessageStore.Messages);
                Assert.Equal("user", user);
                Assert.Equal("password", password);
            }
        }

        [Fact]
        public void ServerCanValidateIncomingClientCertificate()
        {
            // arrange
            var wasConnectedEventRaised = false;

            var clientCertificateValidator = new DelegatingClientCertificateValidator( (sender, certificate, chain, sslPolicyErrors) => true);
            var mockCertificateChain = new X509Certificate2Collection(new X509Certificate2(Convert.FromBase64String(Base64Certificate), CertificatePassword));

            var smtpClient = MailClient.Client();

            smtpClient.ServerCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true;
            smtpClient.ClientCertificates = mockCertificateChain;

            using (CreateServer(endpoint => endpoint.AllowUnsecureAuthentication().Certificate(new X509Certificate2(Convert.FromBase64String(Base64Certificate), CertificatePassword)), services => services.Add(clientCertificateValidator)))
            {
                // act
                 smtpClient.Connect(onConnected: (s, e) => wasConnectedEventRaised = true, options: SecureSocketOptions.StartTls);

                // assert
                Assert.True(wasConnectedEventRaised);
            }
        }

        [Fact]
        public void ServerRefusesIncorrectIncomingClientCertificate()
        {
            // arrange
            var wasConnectedEventRaised = false;

            var clientCertificateValidator = new DelegatingClientCertificateValidator((sender, certificate, chain, sslPolicyErrors) => !string.IsNullOrWhiteSpace(certificate.Subject));
            var mockCertificateChain = new X509Certificate2Collection(new X509Certificate2(new X509Certificate2()));

            var smtpClient = MailClient.Client();
            smtpClient.ServerCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true;
            smtpClient.ClientCertificates = mockCertificateChain;

            using (CreateServer(endpoint => endpoint.AllowUnsecureAuthentication().Certificate(new X509Certificate2(Convert.FromBase64String(Base64Certificate), CertificatePassword)), services => services.Add(clientCertificateValidator)))
            {
                // act & assert
                Assert.Throws<IOException>(() => smtpClient.Connect(onConnected: (s, e) => wasConnectedEventRaised = true, options: SecureSocketOptions.StartTls));
                Assert.False(wasConnectedEventRaised);
            }
        }

        [Fact]
        public void ServerRefusesMissingIncomingClientCertificate()
        {
            // arrange
            var wasConnectedEventRaised = false;

            var clientCertificateValidator = new DelegatingClientCertificateValidator((sender, certificate, chain, sslPolicyErrors) => chain is not null || certificate is not null);

            var smtpClient = MailClient.Client();
            smtpClient.ServerCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true;

            using (CreateServer(endpoint => endpoint.AllowUnsecureAuthentication().Certificate(new X509Certificate2(Convert.FromBase64String(Base64Certificate), CertificatePassword)), services => services.Add(clientCertificateValidator)))
            {
                // act & assert
                Assert.Throws<IOException>(() => smtpClient.Connect(onConnected: (s, e) => wasConnectedEventRaised = true, options: SecureSocketOptions.StartTls));
                Assert.False(wasConnectedEventRaised);
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

            using (CreateServer(endpoint => endpoint.AllowUnsecureAuthentication(), services => services.Add(userAuthenticator)))
            {
                // act and assert
                Assert.Throws<MailKit.Security.AuthenticationException>(() => MailClient.Send(user: user, password: password));

                // assert
                Assert.Empty(MessageStore.Messages);
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
                Assert.Single(MessageStore.Messages);
                Assert.Equal("test1@test.com", MessageStore.Messages[0].Transaction.From.AsAddress());
                Assert.Equal(3, MessageStore.Messages[0].Transaction.To.Count);
                Assert.Equal("test2@test.com", MessageStore.Messages[0].Transaction.To[0].AsAddress());
                Assert.Equal("test3@test.com", MessageStore.Messages[0].Transaction.To[1].AsAddress());
                Assert.Equal("test4@test.com", MessageStore.Messages[0].Transaction.To[2].AsAddress());
            }
        }

        [Fact(Skip = "Command timeout wont work properly until https://github.com/dotnet/corefx/issues/15033")]
        public void WillTimeoutWaitingForCommand()
        {
            using (CreateServer(c => c.CommandWaitTimeout(TimeSpan.FromSeconds(1))))
            {
                var client = MailClient.Client().Connect();
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
                return true;
#pragma warning restore 162
            });

            using (CreateServer(services => services.Add(mailboxFilter)))
            {
                using var client = MailClient.Client().Connect();

                Assert.Throws<ServiceNotAuthenticatedException>(() => client.Send(MailClient.Message()));

                client.NoOp();
            }
        }

        [Fact]
        public void CanReturnSmtpResponseException_SessionWillQuit()
        {
            // arrange
            var mailboxFilter = new DelegatingMailboxFilter(@from => throw new SmtpResponseException(SmtpResponse.AuthenticationRequired, true));

            using (CreateServer(services => services.Add(mailboxFilter)))
            {
                using var client = MailClient.Client().Connect();

                Assert.Throws<ServiceNotAuthenticatedException>(() => client.Send(MailClient.Message()));

                // no longer connected to this is invalid
                Assert.ThrowsAny<Exception>(() => client.NoOp());
            }
        }

        [Fact]
        public void CanForceUserAuthentication_DoesNotThrowIfLoginIsSent()
        {
            var userAuthenticator = new DelegatingUserAuthenticator((user, password) => true);

            using (CreateServer(endpoint => endpoint.AllowUnsecureAuthentication().AuthenticationRequired(), services => services.Add(userAuthenticator)))
            {
                MailClient.Send(user: "user", password: "password");
            }
        }

        [Fact]
        public void CanForceUserAuthentication_ThrowsIfLoginIsNotSent()
        {
            var userAuthenticator = new DelegatingUserAuthenticator((user, password) => true);

            using (CreateServer(endpoint => endpoint.AllowUnsecureAuthentication().AuthenticationRequired(), services => services.Add(userAuthenticator)))
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

                Assert.False(sessionContext.Pipe.IsSecure);
            }

            ServicePointManager.ServerCertificateValidationCallback = null;
        }

        [Fact]
        public void SecuresTheSessionWhenCertificateIsSupplied()
        {
            ServicePointManager.ServerCertificateValidationCallback = IgnoreCertificateValidationFailureForTestingOnly;

            using (var disposable = CreateServer(options => options.Certificate(CreateCertificate())))
            {
                var isSecure = false;
                var sessionCreatedHandler = new EventHandler<SessionEventArgs>(
                    delegate (object sender, SessionEventArgs args)
                    {
                        args.Context.CommandExecuted += (_, commandArgs) =>
                        {
                            isSecure = commandArgs.Context.Pipe.IsSecure;
                        };
                    });

                disposable.Server.SessionCreated += sessionCreatedHandler;

                MailClient.Send();

                disposable.Server.SessionCreated -= sessionCreatedHandler;

                Assert.True(isSecure);
            }

            ServicePointManager.ServerCertificateValidationCallback = null;
        }

        [Fact]
        public void SecuresTheSessionByDefault()
        {
            ServicePointManager.ServerCertificateValidationCallback = IgnoreCertificateValidationFailureForTestingOnly;

            using (var disposable = CreateServer(endpoint => endpoint.IsSecure(true).Certificate(CreateCertificate())))
            {
                var isSecure = false;
                var sessionCreatedHandler = new EventHandler<SessionEventArgs>(
                    delegate (object sender, SessionEventArgs args)
                    {
                        args.Context.CommandExecuted += (_, commandArgs) =>
                        {
                            isSecure = commandArgs.Context.Pipe.IsSecure;
                        };
                    });

                disposable.Server.SessionCreated += sessionCreatedHandler;

                MailClient.NoOp(MailKit.Security.SecureSocketOptions.SslOnConnect);

                disposable.Server.SessionCreated -= sessionCreatedHandler;

                Assert.True(isSecure);
            }

            ServicePointManager.ServerCertificateValidationCallback = null;
        }

        [Fact]
        public void ServerCanBeSecuredAndAuthenticated()
        {
            var userAuthenticator = new DelegatingUserAuthenticator((user, password) => true);

            ServicePointManager.ServerCertificateValidationCallback = IgnoreCertificateValidationFailureForTestingOnly;

            using (var disposable = CreateServer(
                endpoint => endpoint.AllowUnsecureAuthentication(true).Certificate(CreateCertificate()).SupportedSslProtocols(SslProtocols.Tls12),
                services => services.Add(userAuthenticator)))
            {
                var isSecure = false;
                ISessionContext sessionContext = null;
                var sessionCreatedHandler = new EventHandler<SessionEventArgs>(
                    delegate (object sender, SessionEventArgs args)
                    {
                        sessionContext = args.Context;
                        sessionContext.CommandExecuted += (_, commandArgs) =>
                        {
                            isSecure = commandArgs.Context.Pipe.IsSecure;
                        };
                    });

                disposable.Server.SessionCreated += sessionCreatedHandler;

                MailClient.Send(user: "user", password: "password");

                disposable.Server.SessionCreated -= sessionCreatedHandler;

                Assert.True(isSecure);
                Assert.True(sessionContext.Authentication.IsAuthenticated);
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

            using (CreateServer(services => services.Add(endpointListenerFactory)))
            {
                MailClient.Send();
            }

            Assert.True(started);
            Assert.True(stopped);
        }

        [Fact]
        public async Task ListenerCreatedAndListenerCreatedEventRaised()
        {
            // Arrange
            var isListenerStarted = false;
            var options = new SmtpServerOptionsBuilder()
                 .Endpoint(endpointBuilder =>
                 {
                     endpointBuilder.Port(9025);
                     endpointBuilder.AllowUnsecureAuthentication();
                 });

            var serviceProvider = new ServiceProvider();

            var server = new SmtpServer(options.Build(), serviceProvider);
            server.ListenerCreated += (sender, e) => { isListenerStarted = true; };

            // Act
            try
            {
                // StartAsync(...) is a blocking call so we have to avoid awaiting it
                server.StartAsync(CancellationTokenSource.Token);
            }
            catch
            {
                // ignored
            }

            await Task.Delay(TimeSpan.FromSeconds(1));
            CancellationTokenSource.Cancel();

            // Assert
            Assert.True(isListenerStarted);
        }

        [Fact]
        public async Task ListenerFaultedAndListenerFaultedEventRaised()
        {
            // Arrange
            var isListenerFaulted = false;
            var options = new SmtpServerOptionsBuilder()
                 .Endpoint(
                     endpointBuilder =>
                     {
                         endpointBuilder.Port(9025);
                         endpointBuilder.AllowUnsecureAuthentication();
                     });

            var serviceProvider = new ServiceProvider();

            var server = new SmtpServer(options.Build(), serviceProvider);
            server.ListenerFaulted += (sender, e) => { isListenerFaulted = true; };

            // We don't have to await any tasks because we are only looking for the event invocation
            // StartAsync(...) is a blocking call so we have to avoid awaiting it
            server.StartAsync(CancellationTokenSource.Token);
            await Task.Delay(TimeSpan.FromSeconds(1));

            // Act & Assert
            Assert.ThrowsAsync<SocketException>(() => server.StartAsync(CancellationTokenSource.Token));

            await Task.Delay(TimeSpan.FromSeconds(1));
            CancellationTokenSource.Cancel();

            Assert.True(isListenerFaulted);
        }

        public static bool IgnoreCertificateValidationFailureForTestingOnly(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }

        public static X509Certificate2 CreateCertificate()
        {
            // I decided to replace hardcoded path to certificate with Base64String test certificate :)
            // Let me know if you disagree with this change :)
            // var certificate = File.ReadAllBytes(@"C:\Users\caino\Dropbox\Documents\Cain\Programming\SmtpServer\SmtpServer.pfx");
            // var password = File.ReadAllText(@"C:\Users\caino\Dropbox\Documents\Cain\Programming\SmtpServer\SmtpServerPassword.txt");

            return new X509Certificate2(Convert.FromBase64String(Base64Certificate), CertificatePassword);
        }

        /// <summary>
        /// Create a running instance of a server.
        /// </summary>
        /// <returns>A disposable instance which will close and release the server instance.</returns>
        SmtpServerDisposable CreateServer()
        {
            return CreateServer(_ => { }, _ => { }, _ => { });
        }

        /// <summary>
        /// Create a running instance of a server.
        /// </summary>
        /// <param name="serverConfiguration">The configuration to apply to run the server.</param>
        /// <returns>A disposable instance which will close and release the server instance.</returns>
        SmtpServerDisposable CreateServer(Action<SmtpServerOptionsBuilder> serverConfiguration)
        {
            return CreateServer(serverConfiguration, endpointConfiguration => { }, services => { });
        }

        /// <summary>
        /// Create a running instance of a server.
        /// </summary>
        /// <param name="endpointConfiguration">The configuration to apply to run the server.</param>
        /// <returns>A disposable instance which will close and release the server instance.</returns>
        SmtpServerDisposable CreateServer(Action<EndpointDefinitionBuilder> endpointConfiguration)
        {
            return CreateServer(server => { }, endpointConfiguration, services => { });
        }

        /// <summary>
        /// Create a running instance of a server.
        /// </summary>
        /// <param name="serverConfiguration">The configuration to apply to run the server.</param>
        /// <param name="endpointConfiguration">The configuration to apply to the endpoint.</param>
        /// <returns>A disposable instance which will close and release the server instance.</returns>
        SmtpServerDisposable CreateServer(Action<SmtpServerOptionsBuilder> serverConfiguration, Action<EndpointDefinitionBuilder> endpointConfiguration)
        {
            return CreateServer(serverConfiguration, endpointConfiguration, services => { });
        }

        /// <summary>
        /// Create a running instance of a server.
        /// </summary>
        /// <param name="endpointConfiguration">The configuration to apply to the endpoint.</param>
        /// <param name="serviceConfiguration">The configuration to apply to the services.</param>
        /// <returns>A disposable instance which will close and release the server instance.</returns>
        SmtpServerDisposable CreateServer(Action<EndpointDefinitionBuilder> endpointConfiguration, Action<ServiceProvider> serviceConfiguration)
        {
            return CreateServer(server => { }, endpointConfiguration, serviceConfiguration);
        }

        /// <summary>
        /// Create a running instance of a server.
        /// </summary>
        /// <param name="serviceConfiguration">The configuration to apply to the services.</param>
        /// <returns>A disposable instance which will close and release the server instance.</returns>
        SmtpServerDisposable CreateServer(Action<ServiceProvider> serviceConfiguration)
        {
            return CreateServer(server => { }, endpoint => { }, serviceConfiguration);
        }

        /// <summary>
        /// Create a running instance of a server.
        /// </summary>
        /// <param name="serverConfiguration">The configuration to apply to run the server.</param>
        /// <param name="endpointConfiguration">The configuration to apply to the endpoint.</param>
        /// <param name="serviceConfiguration">The configuration to apply to the services.</param>
        /// <returns>A disposable instance which will close and release the server instance.</returns>
        SmtpServerDisposable CreateServer(
            Action<SmtpServerOptionsBuilder> serverConfiguration,
            Action<EndpointDefinitionBuilder> endpointConfiguration,
            Action<ServiceProvider> serviceConfiguration)
        {
            var options = new SmtpServerOptionsBuilder()
                .ServerName("localhost")
                .Endpoint(
                    endpointBuilder =>
                    {
                        endpointBuilder.Port(9025);
                        endpointConfiguration(endpointBuilder);
                    });

            serverConfiguration(options);

            var serviceProvider = new ServiceProvider();
            serviceProvider.Add(MessageStore);
            serviceConfiguration?.Invoke(serviceProvider);

            var server = new SmtpServer(options.Build(), serviceProvider);
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
using System;
using System.IO;
using System.IO.Pipelines;
using System.Net.Security;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using SmtpServer;
using SmtpServer.ComponentModel;
using SmtpServer.IO;
using SmtpServer.Net;
using SmtpServer.Text;

namespace SampleApp.Examples
{
    public static class CustomEndpointListenerExample
    {
        public static void Run()
        {
            var cancellationTokenSource = new CancellationTokenSource();

            var options = new SmtpServerOptionsBuilder()
                .ServerName("SmtpServer SampleApp")
                .Endpoint(builder =>
                    builder
                        .Port(9025, true)
                        .AllowUnsecureAuthentication(false)
                        .Certificate(CreateCertificate()))
                .Build();

            var serviceProvider = new ServiceProvider();
            serviceProvider.Add(new CustomEndpointListenerFactory());

            var server = new SmtpServer.SmtpServer(options, serviceProvider);

            var serverTask = server.StartAsync(cancellationTokenSource.Token);

            SampleMailClient.Send(useSsl: true);

            cancellationTokenSource.Cancel();
            serverTask.WaitWithoutException();
        }

        public sealed class CustomEndpointListenerFactory : EndpointListenerFactory
        {
            public override IEndpointListener CreateListener(IEndpointDefinition endpointDefinition)
            {
                return new CustomEndpointListener(base.CreateListener(endpointDefinition));
            }
        }

        public sealed class CustomEndpointListener : IEndpointListener
        {
            readonly IEndpointListener _endpointListener;

            public CustomEndpointListener(IEndpointListener endpointListener)
            {
                _endpointListener = endpointListener;
            }

            public void Dispose()
            {
                _endpointListener.Dispose();
            }

            public async Task<ISecurableDuplexPipe> GetPipeAsync(ISessionContext context, CancellationToken cancellationToken)
            {
                var pipe = await _endpointListener.GetPipeAsync(context, cancellationToken);

                return new CustomSecurableDuplexPipe(pipe);
            }
        }

        public sealed class CustomSecurableDuplexPipe : ISecurableDuplexPipe
        {
            readonly ISecurableDuplexPipe _securableDuplexPipe;

            public CustomSecurableDuplexPipe(ISecurableDuplexPipe securableDuplexPipe)
            {
                _securableDuplexPipe = securableDuplexPipe;
            }

            public Task UpgradeAsync(X509Certificate certificate, SslProtocols protocols, CancellationToken cancellationToken = default)
            {
                return _securableDuplexPipe.UpgradeAsync(certificate, protocols, cancellationToken);
            }

            public void Dispose()
            {
                _securableDuplexPipe.Dispose();
            }

            public PipeReader Input => new LoggingPipeReader(_securableDuplexPipe.Input);

            public PipeWriter Output => _securableDuplexPipe.Output;

            public bool IsSecure => _securableDuplexPipe.IsSecure;

            /// <inheritdoc />
            public SslProtocols SslProtocol => (_securableDuplexPipe as SslStream)?.SslProtocol ?? SslProtocols.None;
        }

        public sealed class LoggingPipeReader : PipeReader
        {
            readonly PipeReader _delegate;

            public LoggingPipeReader(PipeReader @delegate)
            {
                _delegate = @delegate;
            }

            public override void AdvanceTo(SequencePosition consumed)
            {
                _delegate.AdvanceTo(consumed);
            }

            public override void AdvanceTo(SequencePosition consumed, SequencePosition examined)
            {
                _delegate.AdvanceTo(consumed, examined);
            }

            public override void CancelPendingRead()
            {
                _delegate.CancelPendingRead();
            }

            public override void Complete(Exception exception = null)
            {
                _delegate.Complete(exception);
            }

            public override async ValueTask<ReadResult> ReadAsync(CancellationToken cancellationToken = default(CancellationToken))
            {
                var readResult = await _delegate.ReadAsync(cancellationToken);

                Console.WriteLine(">>> {0}", StringUtil.Create(readResult.Buffer));

                return readResult;
            }

            public override bool TryRead(out ReadResult result)
            {
                return _delegate.TryRead(out result);
            }
        }

        static X509Certificate2 CreateCertificate()
        {
            // to create an X509Certificate for testing you need to run MAKECERT.EXE and then PVK2PFX.EXE
            // http://www.digitallycreated.net/Blog/38/using-makecert-to-create-certificates-for-development

            var certificate = File.ReadAllBytes(@"C:\Users\caino\Dropbox\Documents\Cain\Programming\SmtpServer\SmtpServer.pfx");
            var password = File.ReadAllText(@"C:\Users\caino\Dropbox\Documents\Cain\Programming\SmtpServer\SmtpServerPassword.txt");

            return new X509Certificate2(certificate, password);
        }
    }
}
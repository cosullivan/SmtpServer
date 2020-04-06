using System;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using SmtpServer;
using SmtpServer.IO;
using SmtpServer.Net;

namespace SampleApp.Examples
{
    public static class CustomEndpointListenerExample
    {
        public static void Run()
        {
            var cancellationTokenSource = new CancellationTokenSource();

            var options = new SmtpServerOptionsBuilder()
                .ServerName("SmtpServer SampleApp")
                .Port(9025)
                .EndpointListenerFactory(new CustomEndpointListenerFactory())
                .Build();

            var server = new SmtpServer.SmtpServer(options);

            var serverTask = server.StartAsync(cancellationTokenSource.Token);

            SampleMailClient.Send();

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

            public async Task<INetworkStream> GetStreamAsync(ISessionContext context, CancellationToken cancellationToken)
            {
                var stream = await _endpointListener.GetStreamAsync(context, cancellationToken);

                return new CustomNetworkStream(stream);
            }
        }

        public sealed class CustomNetworkStream : INetworkStream
        {
            readonly INetworkStream _innerStream;

            public CustomNetworkStream(INetworkStream innerStream)
            {
                _innerStream = innerStream;
            }

            public void Dispose()
            {
                _innerStream.Dispose();
            }

            public Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
            {
                Console.WriteLine("Writing {0} bytes to the stream.", count);

                return _innerStream.WriteAsync(buffer, offset, count, cancellationToken);
            }

            public async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
            {
                var bytesRead = await _innerStream.ReadAsync(buffer, offset, count, cancellationToken);

                Console.WriteLine("Read {0} bytes from the stream.", bytesRead);

                return bytesRead;
            }

            public Task FlushAsync(CancellationToken cancellationToken = default)
            {
                return _innerStream.FlushAsync(cancellationToken);
            }

            public Task UpgradeAsync(X509Certificate certificate, SslProtocols protocols, CancellationToken cancellationToken = default)
            {
                return _innerStream.UpgradeAsync(certificate, protocols, cancellationToken);
            }

            public bool IsSecure => _innerStream.IsSecure;
        }
    }
}
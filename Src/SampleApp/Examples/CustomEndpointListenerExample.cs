//using System;
//using System.IO;
//using System.Security.Authentication;
//using System.Security.Cryptography.X509Certificates;
//using System.Text;
//using System.Threading;
//using System.Threading.Tasks;
//using SmtpServer;
//using SmtpServer.IO;
//using SmtpServer.Net;

//namespace SampleApp.Examples
//{
//    public static class CustomEndpointListenerExample
//    {
//        public static void Run()
//        {
//            var cancellationTokenSource = new CancellationTokenSource();

//            var options = new SmtpServerOptionsBuilder()
//                .ServerName("SmtpServer SampleApp")
//                .Certificate(CreateCertificate())
//                .Endpoint(builder =>
//                    builder
//                        .Port(9025, true)
//                        .AllowUnsecureAuthentication(false))
//                .EndpointListenerFactory(new CustomEndpointListenerFactory())
//                .Build();

//            var server = new SmtpServer.SmtpServer(options);

//            var serverTask = server.StartAsync(cancellationTokenSource.Token);

//            SampleMailClient.Send(useSsl: true);

//            cancellationTokenSource.Cancel();
//            serverTask.WaitWithoutException();
//        }

//        public sealed class CustomEndpointListenerFactory : EndpointListenerFactory
//        {
//            public override IEndpointListener CreateListener(IEndpointDefinition endpointDefinition)
//            {
//                return new CustomEndpointListener(base.CreateListener(endpointDefinition));
//            }
//        }

//        public sealed class CustomEndpointListener : IEndpointListener
//        {
//            readonly IEndpointListener _endpointListener;

//            public CustomEndpointListener(IEndpointListener endpointListener)
//            {
//                _endpointListener = endpointListener;
//            }

//            public void Dispose()
//            {
//                _endpointListener.Dispose();
//            }

//            public Task<ISecurableDuplexPipe> GetPipeAsync(ISessionContext context, CancellationToken cancellationToken)
//            {
//                //    var pipe = await _endpointListener.GetPipeAsync(context, cancellationToken);

//                //    return new CustomDuplexPipe(pipe);

//                throw new NotImplementedException();
//            }
//        }

//        public sealed class CustomNetworkStream : INetworkStream
//        {
//            readonly INetworkStream _innerStream;

//            public CustomNetworkStream(INetworkStream innerStream)
//            {
//                _innerStream = innerStream;
//            }

//            public void Dispose()
//            {
//                _innerStream.Dispose();
//            }

//            public Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
//            {
//                Console.WriteLine(Encoding.ASCII.GetString(buffer, offset, count));

//                return _innerStream.WriteAsync(buffer, offset, count, cancellationToken);
//            }

//            public async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
//            {
//                var bytesRead = await _innerStream.ReadAsync(buffer, offset, count, cancellationToken);

//                Console.WriteLine(Encoding.ASCII.GetString(buffer, offset, count));

//                return bytesRead;
//            }

//            public Task FlushAsync(CancellationToken cancellationToken = default)
//            {
//                return _innerStream.FlushAsync(cancellationToken);
//            }

//            public Task UpgradeAsync(X509Certificate certificate, SslProtocols protocols, CancellationToken cancellationToken = default)
//            {
//                Console.WriteLine("Upgrading the stream to SSL");

//                return _innerStream.UpgradeAsync(certificate, protocols, cancellationToken);
//            }

//            public bool IsSecure => _innerStream.IsSecure;
//        }

//        static X509Certificate2 CreateCertificate()
//        {
//            // to create an X509Certificate for testing you need to run MAKECERT.EXE and then PVK2PFX.EXE
//            // http://www.digitallycreated.net/Blog/38/using-makecert-to-create-certificates-for-development

//            var certificate = File.ReadAllBytes(@"C:\Users\cain\Dropbox\Documents\Cain\Programming\SmtpServer\SmtpServer.pfx");
//            var password = File.ReadAllText(@"C:\Users\cain\Dropbox\Documents\Cain\Programming\SmtpServer\SmtpServerPassword.txt");

//            return new X509Certificate2(certificate, password);
//        }
//    }
//}
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using SmtpServer;
using SmtpServer.Net;
using SmtpServer.Tracing;

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

                //return base.CreateListener(endpointDefinition);
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

            public async Task<Stream> GetStreamAsync(ISessionContext context, CancellationToken cancellationToken)
            {
                var stream = await _endpointListener.GetStreamAsync(context, cancellationToken);

                return new CustomStream(stream);
            }
        }

        public sealed class CustomStream : Stream
        {
            readonly Stream _stream;

            public CustomStream(Stream stream)
            {
                _stream = stream;
            }

            public override void Flush()
            {
                _stream.Flush();
            }

            public override long Seek(long offset, SeekOrigin origin)
            {
                return _stream.Seek(offset, origin);
            }

            public override void SetLength(long value)
            {
                _stream.SetLength(value);
            }

            public override int Read(byte[] buffer, int offset, int count)
            {
                var bytesRead = _stream.Read(buffer, offset, count);

                Console.WriteLine("Read {0} bytes from the stream.", bytesRead);

                return bytesRead;
            }

            public override void Write(byte[] buffer, int offset, int count)
            {
                Console.WriteLine("Writing {0} bytes to the stream.", count);

                _stream.Write(buffer, offset, count);
            }

            public override bool CanRead => _stream.CanRead;

            public override bool CanSeek => _stream.CanSeek;

            public override bool CanWrite => _stream.CanWrite;

            public override long Length => _stream.Length;

            public override long Position
            {
                get => _stream.Position;
                set => _stream.Position = value;
            }

            public override int ReadTimeout
            {
                get => _stream.ReadTimeout;
                set => _stream.ReadTimeout = value;
            }
        }
    }
}
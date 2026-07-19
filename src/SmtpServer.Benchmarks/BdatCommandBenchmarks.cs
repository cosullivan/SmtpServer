using System;
using System.Buffers;
using System.IO.Pipelines;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using SmtpServer.ComponentModel;
using SmtpServer.IO;
using SmtpServer.Mail;
using SmtpServer.Protocol;
using SmtpServer.Storage;

namespace SmtpServer.Benchmarks
{
    [MemoryDiagnoser]
    [ShortRunJob]
    public class BdatCommandBenchmarks
    {
        byte[] _message;

        public enum StoreMode
        {
            BufferedMaterializing,
            StreamingDrain
        }

        [Params(StoreMode.BufferedMaterializing, StoreMode.StreamingDrain)]
        public StoreMode Mode { get; set; }

        [Params(1024, 65536)]
        public int BodySize { get; set; }

        [GlobalSetup]
        public void Setup()
        {
            _message = CreateMessage(BodySize);
        }

        [Benchmark]
        public async Task BdatLastChunk()
        {
            var input = new Pipe();
            var output = new Pipe();

            input.Writer.Write(_message);
            await input.Writer.CompleteAsync().ConfigureAwait(false);

            var context = CreateContext(input.Reader, output.Writer);
            var command = new BdatCommand(_message.Length, true);

            await command.ExecuteAsync(context, CancellationToken.None).ConfigureAwait(false);
            await output.Writer.CompleteAsync().ConfigureAwait(false);

            while (true)
            {
                var result = await output.Reader.ReadAsync().ConfigureAwait(false);
                output.Reader.AdvanceTo(result.Buffer.End);

                if (result.IsCompleted)
                {
                    break;
                }
            }

            await output.Reader.CompleteAsync().ConfigureAwait(false);
        }

        SmtpSessionContext CreateContext(PipeReader input, PipeWriter output)
        {
            var endpointDefinition = new EndpointDefinitionBuilder().Build();
            var options = new SmtpServerOptionsBuilder()
                .ServerName("localhost")
                .Endpoint(endpointDefinition)
                .Build();

            var serviceProvider = new ServiceProvider();
            serviceProvider.Add(Mode == StoreMode.StreamingDrain
                ? (IMessageStore)new StreamingDrainMessageStore()
                : new BufferedMaterializingMessageStore());

            var context = new SmtpSessionContext(serviceProvider, options, endpointDefinition)
            {
                Pipe = new BenchmarkDuplexPipe(input, output)
            };

            context.Transaction.From = new Mailbox("sender@example.com");
            context.Transaction.To.Add(new Mailbox("recipient@example.com"));

            return context;
        }

        static byte[] CreateMessage(int bodySize)
        {
            var headers = Encoding.ASCII.GetBytes("From: sender@example.com\r\nTo: recipient@example.com\r\nSubject: BDAT benchmark\r\n\r\n");
            var body = Encoding.ASCII.GetBytes(new string('x', bodySize));
            var message = new byte[headers.Length + body.Length];

            headers.CopyTo(message, 0);
            body.CopyTo(message, headers.Length);

            return message;
        }

        sealed class BenchmarkDuplexPipe : ISecurableDuplexPipe
        {
            public BenchmarkDuplexPipe(PipeReader input, PipeWriter output)
            {
                Input = input;
                Output = output;
            }

            public PipeReader Input { get; }

            public PipeWriter Output { get; }

            public bool IsSecure => false;

            public SslProtocols SslProtocol => SslProtocols.None;

            public Task UpgradeAsync(X509Certificate certificate, SslProtocols protocols, CancellationToken cancellationToken = default)
            {
                throw new NotSupportedException();
            }

            public void Dispose()
            {
            }
        }

        sealed class BufferedMaterializingMessageStore : MessageStore
        {
            public override Task<SmtpResponse> SaveAsync(ISessionContext context, IMessageTransaction transaction, ReadOnlySequence<byte> buffer, CancellationToken cancellationToken)
            {
                _ = buffer.ToArray();

                return Task.FromResult(SmtpResponse.Ok);
            }
        }

        sealed class StreamingDrainMessageStore : MessageStore, IStreamingMessageStore
        {
            public override Task<SmtpResponse> SaveAsync(ISessionContext context, IMessageTransaction transaction, ReadOnlySequence<byte> buffer, CancellationToken cancellationToken)
            {
                throw new NotSupportedException();
            }

            public async Task<SmtpResponse> SaveAsync(ISessionContext context, IMessageTransaction transaction, PipeReader reader, CancellationToken cancellationToken)
            {
                while (true)
                {
                    var result = await reader.ReadAsync(cancellationToken).ConfigureAwait(false);
                    var buffer = result.Buffer;

                    reader.AdvanceTo(buffer.End);

                    if (result.IsCompleted)
                    {
                        break;
                    }
                }

                return SmtpResponse.Ok;
            }
        }
    }
}

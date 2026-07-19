using System;
using System.Buffers;
using System.IO;
using System.IO.Pipelines;
using System.Threading;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using MimeKit;
using SmtpServer.ComponentModel;
using SmtpServer.Protocol;
using SmtpServer.Storage;
using SmtpClient = MailKit.Net.Smtp.SmtpClient;

namespace SmtpServer.Benchmarks
{
    [MemoryDiagnoser]
    [ShortRunJob]
    public class DataStoreBenchmarks
    {
        const int Port = 9026;

        SmtpServer _smtpServer;
        CancellationTokenSource _smtpServerCancellationTokenSource;
        SmtpClient _smtpClient;
        MimeMessage _message;

        public enum StoreMode
        {
            BufferedMaterializing,
            StreamingDrain
        }

        [Params(StoreMode.BufferedMaterializing, StoreMode.StreamingDrain)]
        public StoreMode Mode { get; set; }

        [GlobalSetup]
        public void SmtpServerSetup()
        {
            _message = MimeMessage.Load(typeof(DataStoreBenchmarks).Assembly.GetManifestResourceStream("SmtpServer.Benchmarks.Test3.eml"));
            _smtpServerCancellationTokenSource = new CancellationTokenSource();

            var serviceProvider = new ServiceProvider();
            serviceProvider.Add(Mode == StoreMode.StreamingDrain
                ? (IMessageStore)new StreamingDrainMessageStore()
                : new BufferedMaterializingMessageStore());

            _smtpServer = new SmtpServer(
                new SmtpServerOptionsBuilder()
                    .Port(Port, false)
                    .Build(),
                serviceProvider);

            _ = _smtpServer.StartAsync(_smtpServerCancellationTokenSource.Token);

            _smtpClient = new SmtpClient();
            _smtpClient.Connect("localhost", Port);
        }

        [GlobalCleanup]
        public Task SmtpServerCleanupAsync()
        {
            _smtpClient.Disconnect(true);
            _smtpClient.Dispose();

            _smtpServerCancellationTokenSource.Cancel();
            _smtpServerCancellationTokenSource.Dispose();

            return _smtpServer.ShutdownTask;
        }

        [Benchmark]
        public void SendMessage()
        {
            _smtpClient.Send(_message);
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

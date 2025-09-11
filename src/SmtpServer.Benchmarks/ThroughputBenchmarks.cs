using BenchmarkDotNet.Attributes;
using System.Threading;
using System.Threading.Tasks;
using MimeKit;
using SmtpServer.ComponentModel;
using SmtpClient = MailKit.Net.Smtp.SmtpClient;

namespace SmtpServer.Benchmarks
{
    [MemoryDiagnoser]
    public class ThroughputBenchmarks
    {
        readonly SmtpServer _smtpServer = new SmtpServer(
            new SmtpServerOptionsBuilder()
                .Port(9025, false)
                .Build(),
            ServiceProvider.Default);

        readonly CancellationTokenSource _smtpServerCancellationTokenSource = new CancellationTokenSource();

        readonly SmtpClient _smtpClient = new SmtpClient();

        static readonly MimeMessage Message1 = MimeMessage.Load(typeof(ThroughputBenchmarks).Assembly.GetManifestResourceStream("SmtpServer.Benchmarks.Test1.eml"));
        static readonly MimeMessage Message2 = MimeMessage.Load(typeof(ThroughputBenchmarks).Assembly.GetManifestResourceStream("SmtpServer.Benchmarks.Test2.eml"));
        static readonly MimeMessage Message3 = MimeMessage.Load(typeof(ThroughputBenchmarks).Assembly.GetManifestResourceStream("SmtpServer.Benchmarks.Test3.eml"));
        static readonly MimeMessage Message4 = MimeMessage.Load(typeof(ThroughputBenchmarks).Assembly.GetManifestResourceStream("SmtpServer.Benchmarks.Test4.eml"));

        [GlobalSetup]
        public void SmtpServerSetup()
        {
            _smtpServer.StartAsync(_smtpServerCancellationTokenSource.Token);

            _smtpClient.Connect("localhost", 9025);
        }

        [GlobalCleanup]
        public Task SmtpServerCleanupAsync()
        {
            _smtpClient.Disconnect(true);

            _smtpServerCancellationTokenSource.Cancel();

            return _smtpServer.ShutdownTask;
        }

        [Benchmark]
        public void Send1()
        {
            _smtpClient.Send(Message1);
        }

        [Benchmark]
        public void Send2()
        {
            _smtpClient.Send(Message2);
        }

        [Benchmark]
        public void Send3()
        {
            _smtpClient.Send(Message3);
        }

        [Benchmark]
        public void Send4()
        {
            _smtpClient.Send(Message4);
        }
    }
}

using System.Net.Mail;
using BenchmarkDotNet.Attributes;
using MailKit.Net.Smtp;
using System.Threading;
using System.Threading.Tasks;
using MimeKit;
using SmtpClient = MailKit.Net.Smtp.SmtpClient;

namespace SmtpServer.Benchmarks
{
    public class ThroughputBenchmarks
    {
        readonly SmtpServer _smtpServer = new SmtpServer(
            new SmtpServerOptionsBuilder()
                .Port(9025, false)
                .Build());

        readonly CancellationTokenSource _smtpServerCancellationTokenSource = new CancellationTokenSource();

        readonly SmtpClient _smtpClient = new SmtpClient();

        static readonly MimeMessage _message = MimeMessage.Load(typeof(ThroughputBenchmarks).Assembly.GetManifestResourceStream("SmtpServer.Benchmarks.Test.eml"));

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
        public void NoOp()
        {
            _smtpClient.NoOp();
        }

        [Benchmark]
        public void Send()
        {
            _smtpClient.Send(_message);
        }
    }
}
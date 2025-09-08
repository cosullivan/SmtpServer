using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace WorkerService
{
    public sealed class Worker : BackgroundService
    {
        readonly SmtpServer.SmtpServer _smtpServer;

        public Worker(SmtpServer.SmtpServer smtpServer)
        {
            _smtpServer = smtpServer;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            return _smtpServer.StartAsync(stoppingToken);
        }
    }
}

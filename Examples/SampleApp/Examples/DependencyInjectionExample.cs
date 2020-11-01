using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using SmtpServer;
using SmtpServer.Protocol;

namespace SampleApp.Examples
{
    public static class DependencyInjectionExample
    {
        public static void Run()
        {
            var cancellationTokenSource = new CancellationTokenSource();

            var options = new SmtpServerOptionsBuilder()
                .ServerName("SmtpServer SampleApp")
                .Port(9025)
                .Build();

            var services = new ServiceCollection();
            services.AddSingleton(options);
            services.AddTransient<ISmtpCommandFactory, CustomSmtpCommandFactory>();

            var server = new SmtpServer.SmtpServer(options, services.BuildServiceProvider());

            var serverTask = server.StartAsync(cancellationTokenSource.Token);

            SampleMailClient.Send();

            cancellationTokenSource.Cancel();
            serverTask.WaitWithoutException();
        }

        public sealed class CustomSmtpCommandFactory : SmtpCommandFactory
        {
            public override SmtpCommand CreateEhlo(string domainOrAddress)
            {
                return new CustomEhloCommand(domainOrAddress);
            }
        }

        public sealed class CustomEhloCommand : EhloCommand
        {
            public CustomEhloCommand(string domainOrAddress) : base(domainOrAddress) { }

            protected override string GetGreeting(ISessionContext context)
            {
                return "Good morning, Vietnam!";
            }
        }
    }
}
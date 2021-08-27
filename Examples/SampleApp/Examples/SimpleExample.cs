using System;
using System.Threading;
using SmtpServer;
using SmtpServer.ComponentModel;

namespace SampleApp.Examples
{
    public static class CommonPortsExample
    {
        public static void Run()
        {
            var cancellationTokenSource = new CancellationTokenSource();

            var options = new SmtpServerOptionsBuilder()
                .ServerName("SmtpServer SampleApp")
                .Port(25)
                .Build();

            var serviceProvider = new ServiceProvider();
            serviceProvider.Add(new SampleMessageStore(Console.Out));
            
            var server = new SmtpServer.SmtpServer(options, serviceProvider);
            var serverTask = server.StartAsync(cancellationTokenSource.Token);

            SampleMailClient.Send(); 

            cancellationTokenSource.Cancel();
            serverTask.WaitWithoutException();
        }
    }
}
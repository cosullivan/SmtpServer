using System.Threading;
using MailKit.Net.Smtp;
using MimeKit;
using SmtpServer;

namespace SampleApp.Examples
{
    public static class SimpleExample
    {
        public static void Run()
        {
            var cancellationTokenSource = new CancellationTokenSource();

            var options = new SmtpServerOptionsBuilder()
                .ServerName("SmtpServer SampleApp")
                .Port(9025)
                .Build();

            var server = new SmtpServer.SmtpServer(options);
            var serverTask = server.StartAsync(cancellationTokenSource.Token);

            //SampleMailClient.Send(); 

            var message = MimeMessage.Load(typeof(Program).Assembly.GetManifestResourceStream("SampleApp.Examples.Test.eml"));

            using var client = new SmtpClient();

            client.Connect("localhost", 9025, false);
            client.Send(message);

            cancellationTokenSource.Cancel();
            serverTask.WaitWithoutException();
        }
    }
}
using System.Net;
using System.Threading;
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
                //.Endpoint(new EndpointDefinitionBuilder().Endpoint(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 9025)).Build())
                .Build();

            var server = new SmtpServer.SmtpServer(options);
            var serverTask = server.StartAsync(cancellationTokenSource.Token);

            SampleMailClient.Send(); 

            cancellationTokenSource.Cancel();
            serverTask.WaitWithoutException();
        }
    }
}
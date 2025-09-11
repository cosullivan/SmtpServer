using System;
using System.Threading;
using System.Threading.Tasks;
using SmtpServer;
using SmtpServer.ComponentModel;

namespace SampleApp.Examples
{
    public static class ServerCancellingExample
    {
        public static void Run()
        {
            var cancellationTokenSource = new CancellationTokenSource();

            var options = new SmtpServerOptionsBuilder()
                .ServerName("SmtpServer SampleApp")
                .Port(9025)
                .Build();

            var serviceProvider = new ServiceProvider();
            serviceProvider.Add(new SampleMailboxFilter(TimeSpan.FromSeconds(5)));

            var server = new SmtpServer.SmtpServer(options, serviceProvider);
            server.SessionCreated += OnSessionCreated;
            server.SessionCompleted += OnSessionCompleted;
            server.SessionFaulted += OnSessionFaulted;
            server.SessionCancelled += OnSessionCancelled;

            var serverTask = server.StartAsync(cancellationTokenSource.Token);

            // ReSharper disable once MethodSupportsCancellation
            Task.Run(() => SampleMailClient.Send());

            Console.WriteLine("Press any key to cancel the server.");
            Console.ReadKey();

            Console.WriteLine("Forcibily cancelling the server and any active sessions");

            cancellationTokenSource.Cancel();
            serverTask.WaitWithoutException();

            Console.WriteLine("The server has been cancelled.");
        }

        static void OnSessionCreated(object sender, SessionEventArgs e)
        {
            Console.WriteLine("Session Created.");
        }

        static void OnSessionCompleted(object sender, SessionEventArgs e)
        {
            Console.WriteLine("Session Completed");
        }

        static void OnSessionFaulted(object sender, SessionFaultedEventArgs e)
        {
            Console.WriteLine("Session Faulted: {0}", e.Exception);
        }

        static void OnSessionCancelled(object sender, SessionEventArgs e)
        {
            Console.WriteLine("Session Cancelled");
        }
    }
}
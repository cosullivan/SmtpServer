using System;
using System.Threading;
using SmtpServer;
using SmtpServer.ComponentModel;
using SmtpServer.Tracing;

namespace SampleApp.Examples
{
    public static class SimpleServerExample
    {
        public static void Run()
        {
            var cancellationTokenSource = new CancellationTokenSource();

            var options = new SmtpServerOptionsBuilder()
                .ServerName("SmtpServer SampleApp")
                .Port(9025)
                .CommandWaitTimeout(TimeSpan.FromSeconds(100))
                .Build();

            var server = new SmtpServer.SmtpServer(options, ServiceProvider.Default);
            server.SessionCreated += OnSessionCreated;

            var serverTask = server.StartAsync(cancellationTokenSource.Token);

            Console.WriteLine("Press any key to shutdown the server.");
            Console.ReadKey();

            cancellationTokenSource.Cancel();
            serverTask.WaitWithoutException();
        }

        static void OnSessionCreated(object sender, SessionEventArgs e)
        {
            Console.WriteLine("Session Created.");

            e.Context.CommandExecuting += OnCommandExecuting;
        }

        static void OnCommandExecuting(object sender, SmtpCommandEventArgs e)
        {
            Console.WriteLine("Command Executing.");

            new TracingSmtpCommandVisitor(Console.Out).Visit(e.Command);
        }
    }
}
using System;
using System.Threading;
using SmtpServer;
using SmtpServer.Net;
using SmtpServer.Tracing;

namespace SampleApp.Examples
{
    public static class SessionTracingExample
    {
        static CancellationTokenSource _cancellationTokenSource;

        public static void Run()
        {
            _cancellationTokenSource = new CancellationTokenSource();

            var options = new SmtpServerOptionsBuilder()
                .ServerName("SmtpServer SampleApp")
                .Port(9025)
                .Build();

            var server = new SmtpServer.SmtpServer(options);

            server.SessionCreated += OnSessionCreated;
            server.SessionCompleted += OnSessionCompleted;

            var serverTask = server.StartAsync(_cancellationTokenSource.Token);

            SampleMailClient.Send();

            serverTask.WaitWithoutException();
        }

        static void OnSessionCreated(object sender, SessionEventArgs e)
        {
            Console.WriteLine("SessionCreated: {0}", e.Context.Properties[EndpointListener.RemoteEndPointKey]);

            e.Context.CommandExecuting += OnCommandExecuting;
        }

        static void OnCommandExecuting(object sender, SmtpCommandEventArgs e)
        {
            new TracingSmtpCommandVisitor(Console.Out).Visit(e.Command);
        }

        static void OnSessionCompleted(object sender, SessionEventArgs e)
        {
            Console.WriteLine("SessionCompleted: {0}", e.Context.Properties[EndpointListener.RemoteEndPointKey]);

            e.Context.CommandExecuting -= OnCommandExecuting;

            _cancellationTokenSource.Cancel();
        }
    }
}
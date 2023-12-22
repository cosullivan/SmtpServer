using System;
using System.Text;
using System.Threading;
using SmtpServer;
using SmtpServer.ComponentModel;
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

            var server = new SmtpServer.SmtpServer(options, ServiceProvider.Default);
            
            server.SessionCreated += OnSessionCreated;
            server.SessionCompleted += OnSessionCompleted;
            server.SessionFaulted += OnSessionFaulted;
            server.SessionCancelled += OnSessionCancelled;

            var serverTask = server.StartAsync(_cancellationTokenSource.Token);

            SampleMailClient.Send(recipients: 1000);

            serverTask.WaitWithoutException();
        }

        static void OnSessionFaulted(object sender, SessionFaultedEventArgs e)
        {
            Console.WriteLine("SessionFaulted: {0}", e.Exception);
        }

        static void OnSessionCancelled(object sender, SessionEventArgs e)
        {
            Console.WriteLine("SessionCancelled");
        }

        static void OnSessionCreated(object sender, SessionEventArgs e)
        {
            e.Context.Properties.Add("SessionID", Guid.NewGuid());

            e.Context.CommandExecuting += OnCommandExecuting;
            e.Context.CommandExecuted += OnCommandExecuted;
            e.Context.ResponseException += OnResponseException;
        }

        private static void OnResponseException(object sender, SmtpResponseExceptionEventArgs e)
        {
            Console.WriteLine("Response Exception");
            if (e.Exception.Properties.ContainsKey("SmtpSession:Buffer"))
            {
                var buffer = e.Exception.Properties["SmtpSession:Buffer"] as byte[];
                Console.WriteLine("Unrecognized Command: {0}", Encoding.UTF8.GetString(buffer));
            }
        }

        static void OnCommandExecuting(object sender, SmtpCommandEventArgs e)
        {
            Console.WriteLine("Command Executing (SessionID={0})", e.Context.Properties["SessionID"]);
            new TracingSmtpCommandVisitor(Console.Out).Visit(e.Command);
        }

        static void OnCommandExecuted(object sender, SmtpCommandEventArgs e)
        {
            Console.WriteLine("Command Executed (SessionID={0})", e.Context.Properties["SessionID"]);
            new TracingSmtpCommandVisitor(Console.Out).Visit(e.Command);
        }

        static void OnSessionCompleted(object sender, SessionEventArgs e)
        {
            Console.WriteLine("SessionCompleted: {0}", e.Context.Properties[EndpointListener.RemoteEndPointKey]);

            e.Context.CommandExecuting -= OnCommandExecuting;
            e.Context.CommandExecuted -= OnCommandExecuted;
            e.Context.ResponseException -= OnResponseException;

            _cancellationTokenSource.Cancel();
        }
    }
}
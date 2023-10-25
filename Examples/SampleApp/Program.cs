using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using MimeKit;
using SampleApp.Examples;
using SmtpServer;
using SmtpServer.IO;
using SmtpServer.Protocol;
using SmtpServer.Tests;
using SmtpServer.Text;
using SmtpServer.ComponentModel;
using SmtpServer.Net;
using SmtpServer.Tracing;

namespace SampleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            ServicePointManager.ServerCertificateValidationCallback = IgnoreCertificateValidationFailureForTestingOnly;

            //SimpleExample.Run();
            //SimpleServerExample.Run();
            //CustomEndpointListenerExample.Run();
            //ServerCancellingExample.Run();
            //SessionTracingExample.Run();
            //DependencyInjectionExample.Run();
            //SecureServerExample.Run();

            //_cancellationTokenSource = new CancellationTokenSource();

            var options = new SmtpServerOptionsBuilder()
                .ServerName("SmtpServer SampleApp")
                .Port(9025)
                .Build();

            var serviceProvider = new ServiceProvider();
            serviceProvider.Add(new SampleMessageStore(Console.Out));

            var server = new SmtpServer.SmtpServer(options, serviceProvider);

            server.SessionCreated += OnSessionCreated;
            server.SessionCompleted += OnSessionCompleted;
            server.SessionFaulted += OnSessionFaulted;
            server.SessionCancelled += OnSessionCancelled;

            var serverTask = server.StartAsync(CancellationToken.None);

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
                Console.WriteLine("Unknown Line: {0}", Encoding.UTF8.GetString(buffer));
            }
        }

        static void OnCommandExecuting(object sender, SmtpCommandEventArgs e)
        {
            //Console.WriteLine("Command Executing (SessionID={0})", e.Context.Properties["SessionID"]);
            new TracingSmtpCommandVisitor(Console.Out).Visit(e.Command);
        }

        static void OnCommandExecuted(object sender, SmtpCommandEventArgs e)
        {
            //Console.WriteLine("Command Executed (SessionID={0})", e.Context.Properties["SessionID"]);
            //new TracingSmtpCommandVisitor(Console.Out).Visit(e.Command);
        }

        static void OnSessionCompleted(object sender, SessionEventArgs e)
        {
            Console.WriteLine("SessionCompleted: {0}", e.Context.Properties[EndpointListener.RemoteEndPointKey]);

            e.Context.CommandExecuting -= OnCommandExecuting;
            e.Context.CommandExecuted -= OnCommandExecuted;
            e.Context.ResponseException -= OnResponseException;
        }

        static bool IgnoreCertificateValidationFailureForTestingOnly(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }
    }
}
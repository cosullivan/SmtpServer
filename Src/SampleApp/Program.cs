using System;
using System.Net;
using System.Security.Authentication;
using System.Threading;
using System.Threading.Tasks;
using SampleApp.Examples;
using SmtpServer;
using SmtpServer.Authentication;
using SmtpServer.Net;
using SmtpServer.Tests;
using SmtpServer.Tracing;

namespace SampleApp
{
    class Program
    {
        static async Task Main(string[] args)
        {
            //CustomEndpointListenerExample.Run();

            ServicePointManager.ServerCertificateValidationCallback = SmtpServerTests.IgnoreCertificateValidationFailureForTestingOnly;

            var options = new SmtpServerOptionsBuilder()
                .ServerName("SmtpServer SampleApp")
                .Port(587, false)
                //.Certificate(SmtpServerTests.CreateCertificate())
                .Build();

            //var options = new SmtpServerOptionsBuilder()
            //    .ServerName("SmtpServer SampleApp")
            //    .Endpoint(endpoint =>
            //        endpoint
            //            .Port(587)
            //            .AllowUnsecureAuthentication(true)
            //            .AuthenticationRequired(false))
            //    .UserAuthenticator(new SampleUserAuthenticator())
            //    //.Certificate(SmtpServerTests.CreateCertificate())
            //    .Build();

            var server = new SmtpServer.SmtpServer(options);

            server.SessionCreated += OnSessionCreated;
            server.SessionCompleted += OnSessionCompleted;
            server.SessionFaulted += OnSessionFaulted;

            var serverTask = server.StartAsync(CancellationToken.None);

            Console.WriteLine("Starting, press any key to shutdown.");
            
            Console.ReadKey();

            Console.WriteLine("Shutting down the server.");

            server.Shutdown();

            await server.ShutdownTask;

            await serverTask.ConfigureAwait(false);

            Console.WriteLine("Finished");

            //CustomEndpointListenerExample.Run();
        }

        static void OnSessionFaulted(object sender, SessionFaultedEventArgs e)
        {
            Console.WriteLine("SessionFaulted: {0}", e.Context.Properties[EndpointListener.RemoteEndPointKey]);
            Console.WriteLine(e.Exception.Message);
        }

        static void OnSessionCreated(object sender, SessionEventArgs e)
        {
            Console.WriteLine("SessionCreated: {0}", e.Context.Properties[EndpointListener.RemoteEndPointKey]);

            e.Context.CommandExecuting += OnCommandExecuting;
        }

        static void OnCommandExecuting(object sender, SmtpCommandExecutingEventArgs e)
        {
            new TracingSmtpCommandVisitor(Console.Out).Visit(e.Command);
        }

        static void OnSessionCompleted(object sender, SessionEventArgs e)
        {
            Console.WriteLine("SessionCompleted: {0}", e.Context.Properties[EndpointListener.RemoteEndPointKey]);

            e.Context.CommandExecuting -= OnCommandExecuting;
        }

        //static void Main(string[] args)
        //{
        //    var cancellationTokenSource = new CancellationTokenSource();
            
        //    if (args == null || args.Length == 0)
        //    {
        //        var serverTask = RunServerAsync(cancellationTokenSource.Token);
        //        var clientTask1 = RunClientAsync("A", forceConnection: false, cancellationToken: cancellationTokenSource.Token);
        //        var clientTask2 = RunClientAsync("B", forceConnection: false, cancellationToken: cancellationTokenSource.Token);
        //        var clientTask3 = RunClientAsync("C", forceConnection: false, cancellationToken: cancellationTokenSource.Token);

        //        Console.WriteLine("Press any key to continue");
        //        Console.ReadKey();

        //        cancellationTokenSource.Cancel();

        //        serverTask.WaitWithoutException();
        //        clientTask1.WaitWithoutException();
        //        clientTask2.WaitWithoutException();
        //        clientTask3.WaitWithoutException();

        //        return;
        //    }

        //    if (args[0] == "server")
        //    {
        //        var serverTask = RunServerAsync(cancellationTokenSource.Token);

        //        Console.WriteLine("Press any key to continue");
        //        Console.ReadKey();

        //        cancellationTokenSource.Cancel();

        //        serverTask.WaitWithoutException();

        //        return;
        //    }

        //    if (args[0] == "client")
        //    {
        //        var clientTask = RunClientAsync(args[1], cancellationToken: cancellationTokenSource.Token);

        //        Console.WriteLine("Press any key to continue");
        //        Console.ReadKey();

        //        cancellationTokenSource.Cancel();

        //        clientTask.WaitWithoutException();
        //    }

        //    if (args[0] == "folder")
        //    {
        //        var clientTask = RunClientAsync(args[1], cancellationToken: cancellationTokenSource.Token);

        //        Console.WriteLine("Press any key to continue");
        //        Console.ReadKey();

        //        cancellationTokenSource.Cancel();

        //        clientTask.WaitWithoutException();
        //    }
        //}

        //static async Task RunServerAsync(CancellationToken cancellationToken)
        //{
        //    var options = new SmtpServerOptionsBuilder().Port(9025).Build();

        //    var smtpServer = new SmtpServer.SmtpServer(options);

        //    await smtpServer.StartAsync(cancellationToken).ConfigureAwait(false);
        //}

        //static async Task RunClientAsync(
        //    string name,
        //    int limit = Int32.MaxValue,
        //    bool forceConnection = true,
        //    CancellationToken cancellationToken = default(CancellationToken))
        //{
        //    //var message = MimeKit.MimeMessage.Load(ParserOptions.Default, @"C:\Dev\Enron Corpus\maildir\allen-p\inbox\31_");

        //    var message = new MimeMessage();

        //    message.From.Add(new MailboxAddress("from@sample.com"));
        //    message.To.Add(new MailboxAddress("to@sample.com"));
        //    message.Subject = "Hello";
        //    message.Body = new TextPart("plain")
        //    {
        //        Text = "Hello World"
        //    };

        //    var stopwatch = new Stopwatch();
        //    stopwatch.Start();

        //    var counter = 1;
        //    using (var smtpClient = new SmtpClient())
        //    {
        //        while (limit-- > 0 && cancellationToken.IsCancellationRequested == false)
        //        {
        //            //Console.WriteLine("Name={0} Count={1}", name, counter);

        //            try
        //            {
        //                if (smtpClient.IsConnected == false)
        //                {
        //                    await smtpClient.ConnectAsync("localhost", 9025, false, cancellationToken).ConfigureAwait(false);

        //                    if (smtpClient.Capabilities.HasFlag(SmtpCapabilities.Authentication))
        //                    {
        //                        await smtpClient.AuthenticateAsync("user", "password", cancellationToken).ConfigureAwait(false);
        //                    }
        //                }

        //                await smtpClient.SendAsync(message, cancellationToken).ConfigureAwait(false);
        //            }
        //            catch (Exception exception)
        //            {
        //                if (cancellationToken.IsCancellationRequested)
        //                {
        //                    break;
        //                }
        //            }

        //            if (forceConnection)
        //            {
        //                await smtpClient.DisconnectAsync(true, cancellationToken).ConfigureAwait(false);
        //            }

        //            counter++;
        //        }
        //    }

        //    stopwatch.Stop();

        //    Console.WriteLine();
        //    Console.WriteLine("{0} Finished.", name);
        //    Console.WriteLine("  {0} Messages Sent.", counter);
        //    Console.WriteLine("  {0} Time Taken (ms).", stopwatch.ElapsedMilliseconds);
        //    Console.WriteLine("  {0} Throughput (mps).", counter / (stopwatch.ElapsedMilliseconds / 1000.0));
        //}
    }
}

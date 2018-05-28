using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MailKit.Net.Smtp;
using MimeKit;
using MimeKit.Text;
using SampleApp.Examples;
using SmtpServer;
using SmtpServer.Mail;

namespace SampleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            var cancellationTokenSource = new CancellationTokenSource();
            
            if (args == null || args.Length == 0)
            {
                var serverTask = RunServerAsync(cancellationTokenSource.Token);
                var clientTask1 = RunClientAsync("A", forceConnection: false, cancellationToken: cancellationTokenSource.Token);
                var clientTask2 = RunClientAsync("B", forceConnection: false, cancellationToken: cancellationTokenSource.Token);
                var clientTask3 = RunClientAsync("C", forceConnection: false, cancellationToken: cancellationTokenSource.Token);

                Console.WriteLine("Press any key to continue");
                Console.ReadKey();

                cancellationTokenSource.Cancel();

                serverTask.WaitWithoutException();
                clientTask1.WaitWithoutException();
                clientTask2.WaitWithoutException();
                clientTask3.WaitWithoutException();

                return;
            }

            if (args[0] == "server")
            {
                var serverTask = RunServerAsync(cancellationTokenSource.Token);

                Console.WriteLine("Press any key to continue");
                Console.ReadKey();

                cancellationTokenSource.Cancel();

                serverTask.WaitWithoutException();

                return;
            }

            if (args[0] == "client")
            {
                var clientTask = RunClientAsync(args[1], cancellationToken: cancellationTokenSource.Token);

                Console.WriteLine("Press any key to continue");
                Console.ReadKey();

                cancellationTokenSource.Cancel();

                clientTask.WaitWithoutException();
            }

            if (args[0] == "folder")
            {
                var clientTask = RunClientAsync(args[1], cancellationToken: cancellationTokenSource.Token);

                Console.WriteLine("Press any key to continue");
                Console.ReadKey();

                cancellationTokenSource.Cancel();

                clientTask.WaitWithoutException();
            }
        }

        static async Task RunServerAsync(CancellationToken cancellationToken)
        {
            var options = new SmtpServerOptionsBuilder().Port(9025).Build();

            var smtpServer = new SmtpServer.SmtpServer(options);

            await smtpServer.StartAsync(cancellationToken);
        }

        static async Task RunClientAsync(
            string name,
            int limit = Int32.MaxValue,
            bool forceConnection = true,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            //var message = MimeKit.MimeMessage.Load(ParserOptions.Default, @"C:\Dev\Enron Corpus\maildir\allen-p\inbox\31_");

            var message = new MimeMessage();

            message.From.Add(new MailboxAddress("from@sample.com"));
            message.To.Add(new MailboxAddress("to@sample.com"));
            message.Subject = "Hello";
            message.Body = new TextPart("plain")
            {
                Text = "Hello World"
            };

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            var counter = 1;
            using (var smtpClient = new SmtpClient())
            {
                while (limit-- > 0 && cancellationToken.IsCancellationRequested == false)
                {
                    //Console.WriteLine("Name={0} Count={1}", name, counter);

                    try
                    {
                        if (smtpClient.IsConnected == false)
                        {
                            await smtpClient.ConnectAsync("localhost", 9025, false, cancellationToken);

                            if (smtpClient.Capabilities.HasFlag(SmtpCapabilities.Authentication))
                            {
                                await smtpClient.AuthenticateAsync("user", "password", cancellationToken);
                            }
                        }

                        await smtpClient.SendAsync(message, cancellationToken).ConfigureAwait(false);
                    }
                    catch (Exception exception)
                    {
                        if (cancellationToken.IsCancellationRequested)
                        {
                            break;
                        }
                    }

                    if (forceConnection)
                    {
                        await smtpClient.DisconnectAsync(true, cancellationToken);
                    }

                    counter++;
                }
            }

            stopwatch.Stop();

            Console.WriteLine();
            Console.WriteLine("{0} Finished.", name);
            Console.WriteLine("  {0} Messages Sent.", counter);
            Console.WriteLine("  {0} Time Taken (ms).", stopwatch.ElapsedMilliseconds);
            Console.WriteLine("  {0} Throughput (mps).", counter / (stopwatch.ElapsedMilliseconds / 1000.0));
        }

        //static async Task SendMessageAsync(SmtpClient smtpClient, string name, int counter, CancellationToken cancellationToken = default(CancellationToken))
        //{
        //    var message = new MimeKit.MimeMessage();
        //    message.From.Add(new MimeKit.MailboxAddress($"{name}{counter}@test.com"));
        //    message.To.Add(new MimeKit.MailboxAddress("sample@test.com"));
        //    message.Subject = $"{name} {counter}";

        //    message.Body = new TextPart(TextFormat.Plain)
        //    {
        //        Text = ".Assunto teste acento çãõáéíóú"
        //    };

        //    await smtpClient.SendAsync(message, cancellationToken).ConfigureAwait(false);
        //}

        //static async Task RunFolderAsync(string folder, string pattern = "*", CancellationToken cancellationToken = default(CancellationToken))
        //{
        //    foreach (var directory in Directory.GetDirectories(folder, "*", SearchOption.AllDirectories))
        //    {
        //        Console.WriteLine(directory);
        //        cancellationToken.ThrowIfCancellationRequested();

        //        foreach (var file in Directory.GetFiles(directory, pattern).ToList())
        //        {
        //            cancellationToken.ThrowIfCancellationRequested();
        //            Console.WriteLine(new FileInfo(file).Name);

        //            MimeKit.MimeMessage message;
        //            try
        //            {
        //                message = MimeKit.MimeMessage.Load(ParserOptions.Default, file);
        //            }
        //            catch
        //            {
        //                continue;
        //            }

        //            using (var smtpClient = new SmtpClient())
        //            {
        //                try
        //                {
        //                    await smtpClient.ConnectAsync("localhost", 9025, false, cancellationToken);
        //                    await smtpClient.AuthenticateAsync("user", "password", cancellationToken);

        //                    await smtpClient.SendAsync(message, cancellationToken).ConfigureAwait(false);
        //                }
        //                catch (Exception exception)
        //                {
        //                    Console.WriteLine(exception);
        //                }

        //                await smtpClient.DisconnectAsync(true, cancellationToken);
        //            }
        //        }
        //    }
        //}

        //static async Task RunFileAsync(string file, CancellationToken cancellationToken = default(CancellationToken))
        //{
        //    var message = MimeKit.MimeMessage.Load(ParserOptions.Default, file);

        //    using (var smtpClient = new SmtpClient())
        //    {
        //        try
        //        {
        //            await smtpClient.ConnectAsync("localhost", 9025, false, cancellationToken);
        //            await smtpClient.AuthenticateAsync("user", "password", cancellationToken);

        //            await smtpClient.SendAsync(message, cancellationToken).ConfigureAwait(false);
        //        }
        //        catch (Exception exception)
        //        {
        //            Console.WriteLine(exception);
        //        }

        //        await smtpClient.DisconnectAsync(true, cancellationToken);
        //    }
        //}


        //static void OnSmtpServerSessionCreated(object sender, SessionEventArgs e)
        //{
        //    Console.WriteLine("SessionCreated: {0}", e.Context.RemoteEndPoint);

        //    e.Context.CommandExecuting += OnCommandExecuting;
        //}

        //static void OnCommandExecuting(object sender, SmtpCommandExecutingEventArgs e)
        //{
        //    new TracingSmtpCommandVisitor(Console.Out).Visit(e.Command);
        //}

        //static void OnSmtpServerSessionCompleted(object sender, SessionEventArgs e)
        //{
        //    e.Context.CommandExecuting -= OnCommandExecuting;

        //    Console.WriteLine("SessionCompleted: {0}", e.Context.RemoteEndPoint);
        //}

        //static bool IgnoreCertificateValidationFailureForTestingOnly(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        //{
        //    return true;
        //}

        //static X509Certificate2 CreateCertificate()
        //{
        //    // to create an X509Certificate for testing you need to run MAKECERT.EXE and then PVK2PFX.EXE
        //    // http://www.digitallycreated.net/Blog/38/using-makecert-to-create-certificates-for-development

        //    var certificate = File.ReadAllBytes(@"C:\Dropbox\Documents\Cain\Programming\SmtpServer\SmtpServer.pfx");
        //    var password = File.ReadAllText(@"C:\Dropbox\Documents\Cain\Programming\SmtpServer\SmtpServerPassword.txt");

        //    return new X509Certificate2(certificate, password);
        //}
    }
}
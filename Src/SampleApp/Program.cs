using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MailKit.Net.Smtp;
using SmtpServer;
using SmtpServer.Tracing;
using MimeKit;
using MimeKit.Text;
using SmtpServer.IO;
using SmtpServer.Mime;
using SmtpServer.Tests;
using SmtpServer.Text;
using ISmtpServerOptions = SmtpServer.ISmtpServerOptions;
using MimePart = SmtpServer.Mime.MimePart;
using SmtpClient = MailKit.Net.Smtp.SmtpClient;

namespace SampleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            //@"C:\Dev\Enron Corpus\maildir\allen-p\inbox\31_"
            ////var tests = new MimeMessageSerializerTests();
            ////tests.CanParseMessage();

            ////65,65,65,45,46,46,46
            ////var stream = new System.IO.MemoryStream(Encoding.ASCII.GetBytes("AAAA-123"));
            ////var stream = new System.IO.MemoryStream(new byte[] { 65, 65, 65, 45, 49, 49, 49 });
            //var stream = new System.IO.MemoryStream(new byte[] { 65, 65, 65, 13, 65, 13, 10, 45, 49, 49, 49 });
            //var networkClient = new NetworkClient(stream, 5);

            ////HERE: change from ReadLineASync to ReadBlock?

            //var reader = new ByteArrayTokenReader(networkClient.ReadDotBlockAsync().Result);
            //Token token;
            //while ((token = reader.NextToken()) != Token.None)
            //{
            //    Console.WriteLine(token);
            //}

            ////using (var stream = File.OpenRead(@"C:\Dev\temp\msg.txt"))
            //using (var stream = File.OpenRead(@"C:\Dev\Enron Corpus\maildir\allen-p\inbox\31_"))
            //{
            //    var networkClient = new NetworkClient(stream, 128);
            //    var blocks = networkClient.ReadDotBlockAsync().Result;

            //    var stopwatch = new Stopwatch();
            //    stopwatch.Start();

            //    for (var i = 0; i < 1; i++)
            //    {
            //        var reader = new ByteArrayTokenReader(blocks);
            //        //var tokens = reader.ToList();
            //        //Token token;
            //        //while ((token = reader.NextToken()) != Token.None)
            //        //{
            //        //    Console.WriteLine(token);
            //        //}

            //        var mimeParser = new SmtpServer.Mime.MimeParser(new TokenEnumerator(reader));
            //        Console.WriteLine(mimeParser.TryMakeDocument(out MimeDocument document));

            //        Console.WriteLine(document.Version);
            //        Console.WriteLine(document.Body);

            //        Console.WriteLine(new StreamReader(((MimePart)document.Body).Content).ReadToEnd());
            //    }

            //    stopwatch.Stop();
            //    Console.WriteLine("Time Taken {0}ms", stopwatch.ElapsedMilliseconds);
            //}

            //var file = @"C:\Dev\Enron Corpus\maildir\allen-p\inbox\31_";

            //var stopwatch = new Stopwatch();
            //stopwatch.Start();

            //for (var i = 0; i < 100; i++)
            //{
            //    //using (var stream = File.OpenRead(file))
            //    //{
            //    //    var message = MimeKit.MimeMessage.Load(stream);
            //    //}

            //    using (var stream = File.OpenRead(file))
            //    {
            //        var networkClient = new NetworkClient(stream, 128);
            //        var blocks = networkClient.ReadDotBlockAsync().Result;

            //        var mimeParser = new SmtpServer.Mime.MimeParser(new TokenEnumerator(new ByteArrayTokenReader(blocks)));
            //        mimeParser.TryMakeDocument(out MimeDocument document);
            //    }
            //}

            //stopwatch.Stop();
            //Console.WriteLine("Time Taken {0}ms", stopwatch.ElapsedMilliseconds);
            
            //return;

            var cancellationTokenSource = new CancellationTokenSource();

            var certificate = CreateCertificate();

            ServicePointManager.ServerCertificateValidationCallback = IgnoreCertificateValidationFailureForTestingOnly;

            var options = new OptionsBuilder()
                .ServerName("SmtpServer SampleApp")
                .Port(9025)
                .Certificate(certificate)
                .SupportedSslProtocols(SslProtocols.Default)
                .MessageStore(new SampleMessageStore())
                //.MailboxFilter(new SampleMailboxFilter())
                //.UserAuthenticator(new SampleUserAuthenticator())
                .Build();

            var s = RunServerAsync(options, cancellationTokenSource.Token);
            var c = RunClientAsync("A", 1, false, cancellationTokenSource.Token);
            //var c = RunFolderAsync(@"C:\Dev\Enron Corpus\maildir", CancellationToken.None);

            Console.WriteLine("Press any key to continue");
            Console.ReadKey();

            cancellationTokenSource.Cancel();

            s.WaitWithoutException();
            c.WaitWithoutException();

            return;

            if (args == null || args.Length == 0)
            {
                var serverTask = RunServerAsync(options, cancellationTokenSource.Token);
                var clientTask1 = RunClientAsync("A", cancellationToken: cancellationTokenSource.Token);
                var clientTask2 = RunClientAsync("B", cancellationToken: cancellationTokenSource.Token);
                var clientTask3 = RunClientAsync("C", cancellationToken: cancellationTokenSource.Token);

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
                var serverTask = RunServerAsync(options, cancellationTokenSource.Token);

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

        static async Task RunServerAsync(ISmtpServerOptions options, CancellationToken cancellationToken)
        {
            var smtpServer = new SmtpServer.SmtpServer(options);

            //smtpServer.SessionCreated += OnSmtpServerSessionCreated;
            //smtpServer.SessionCompleted += OnSmtpServerSessionCompleted;

            await smtpServer.StartAsync(cancellationToken);

            //smtpServer.SessionCreated -= OnSmtpServerSessionCreated;
            //smtpServer.SessionCompleted -= OnSmtpServerSessionCompleted;
        }

        static async Task RunClientAsync(
            string name, 
            int limit = Int32.MaxValue, 
            bool forceConnection = true,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            //var message = MimeKit.MimeMessage.Load(ParserOptions.Default, @"C:\Dev\Enron Corpus\maildir\allen-p\inbox\31_");

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            using (var smtpClient = new SmtpClient())
            {
                var counter = 1;
                while (limit-- > 0 && cancellationToken.IsCancellationRequested == false)
                {
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

                        await SendMessageAsync(smtpClient, name, counter, cancellationToken);
                        //await smtpClient.SendAsync(message, cancellationToken).ConfigureAwait(false);
                    }
                    catch (Exception exception)
                    {
                        Console.WriteLine(exception);
                    }

                    if (forceConnection)
                    {
                        await smtpClient.DisconnectAsync(true, cancellationToken);
                    }

                    counter++;
                }
            }

            stopwatch.Stop();

            Console.WriteLine("Finished. Time Taken {0}ms", stopwatch.ElapsedMilliseconds);
        }

        static async Task SendMessageAsync(SmtpClient smtpClient, string name, int counter, CancellationToken cancellationToken = default(CancellationToken))
        {
            //C:\Dev\Enron Corpus\maildir\allen-p\inbox\31_
            var message = new MimeKit.MimeMessage();
            message.From.Add(new MimeKit.MailboxAddress($"{name}{counter}@test.com"));
            message.To.Add(new MimeKit.MailboxAddress("sample@test.com"));
            message.Subject = $"{name} {counter}";

            message.Body = new TextPart(TextFormat.Plain)
            {
                Text = ".Assunto teste acento çãõáéíóú"
            };

            await smtpClient.SendAsync(message, cancellationToken).ConfigureAwait(false);
        }

        static async Task RunFolderAsync(string folder, CancellationToken cancellationToken = default(CancellationToken))
        {
            foreach (var directory in Directory.GetDirectories(folder, "*", SearchOption.AllDirectories))
            {
                Console.WriteLine(directory);
                cancellationToken.ThrowIfCancellationRequested();

                foreach (var file in Directory.GetFiles(directory).ToList())
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    Console.WriteLine(new FileInfo(file).Name);

                    MimeKit.MimeMessage message;
                    try
                    {
                        message = MimeKit.MimeMessage.Load(ParserOptions.Default, file);
                    }
                    catch
                    {
                        continue;
                    }

                    using (var smtpClient = new SmtpClient())
                    {
                        try
                        {
                            await smtpClient.ConnectAsync("localhost", 9025, false, cancellationToken);
                            await smtpClient.AuthenticateAsync("user", "password", cancellationToken);

                            await smtpClient.SendAsync(message, cancellationToken).ConfigureAwait(false);
                        }
                        catch (Exception exception)
                        {
                            Console.WriteLine(exception);
                        }

                        await smtpClient.DisconnectAsync(true, cancellationToken);
                    }
                }
            }
        }

        static void OnSmtpServerSessionCreated(object sender, SessionEventArgs e)
        {
            Console.WriteLine("SessionCreated: {0}", e.Context.RemoteEndPoint);

            e.Context.CommandExecuting += OnCommandExecuting;
        }

        static void OnCommandExecuting(object sender, SmtpCommandExecutingEventArgs e)
        {
            new TracingSmtpCommandVisitor(Console.Out).Visit(e.Command);
        }

        static void OnSmtpServerSessionCompleted(object sender, SessionEventArgs e)
        {
            e.Context.CommandExecuting -= OnCommandExecuting;

            Console.WriteLine("SessionCompleted: {0}", e.Context.RemoteEndPoint);
        }

        static bool IgnoreCertificateValidationFailureForTestingOnly(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }
        
        static X509Certificate2 CreateCertificate()
        {
            // to create an X509Certificate for testing you need to run MAKECERT.EXE and then PVK2PFX.EXE
            // http://www.digitallycreated.net/Blog/38/using-makecert-to-create-certificates-for-development

            var certificate = File.ReadAllBytes(@"C:\Dropbox\Documents\Cain\Programming\SmtpServer\SmtpServer.pfx");
            var password = File.ReadAllText(@"C:\Dropbox\Documents\Cain\Programming\SmtpServer\SmtpServerPassword.txt");

            return new X509Certificate2(certificate, password);
        }
    }
}
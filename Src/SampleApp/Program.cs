using System;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Net.Mime;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SmtpServer;
using SmtpServer.Tracing;
using MailKit.Net.Smtp;
using MimeKit;
using MimeKit.Text;
using SmtpServer.Content;
using SmtpServer.Mime;
using SmtpServer.Text;
using ContentType = SmtpServer.Mime.ContentType;
using SmtpClient = MailKit.Net.Smtp.SmtpClient;

namespace SampleApp
{
    // https://tools.ietf.org/html/rfc2045
    class Program
    {
        static void Main(string[] args)
        {
            //Console.WriteLine(Char.IsPunctuation((char)34));
            //return;
            //var text = "MIME-Version: 1.0";
            //var parser = new SmtpServer.Mime.MimeParser(new TokenEnumerator(new StringTokenReader(text)));

            //parser.TryMakeMimeVersion(out MimeVersion version);
            //Console.WriteLine(version);

            //var text = "Content-Type: text/plain";
            //var parser = new SmtpServer.Mime.MimeParser(new TokenEnumerator(new StringTokenReader(text)));

            //parser.TryMakeContentType(out ContentType contentType);
            //Console.WriteLine(contentType);

            var mimeMessageReader = new MimeMessageReader(File.OpenRead(@"C:\Dev\temp\msg.txt"));
            try
            {
                var mimeMessage = mimeMessageReader.ReadAsync().Result;
                Console.WriteLine(mimeMessage);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }

            //var webClient = new WebClient();
            ////using (var stream = webClient.OpenRead("http://memberzonedev.org"))
            //using (var stream = new System.IO.MemoryStream(Encoding.ASCII.GetBytes("abcd\r\n1234")))
            //{
            //    var reader = new StreamTokenReader(stream, 5);

            //    var enumerator = new TokenEnumerator2(reader, ignoreWhiteSpace: false);

            //    while (enumerator.PeekAsync().Result != Token.None)
            //    {
            //        Console.WriteLine(enumerator.TakeAsync().Result);
            //    }
            //}

            return;

            var cancellationTokenSource = new CancellationTokenSource();

            //var certificate = CreateCertificate();

            //ServicePointManager.ServerCertificateValidationCallback = IgnoreCertificateValidationFailureForTestingOnly;

            var options = new OptionsBuilder()
                .ServerName("SmtpServer SampleApp")
                .Port(9025)
                //.Certificate(certificate)
                .SupportedSslProtocols(SslProtocols.Default)
                .MessageStore(new ConsoleMessageStore())
                .MailboxFilter(new ConsoleMailboxFilter())
                .Build();

            var s = RunServerAsync(options, cancellationTokenSource.Token);
            var c = RunClientAsync("A", cancellationTokenSource.Token);

            Console.WriteLine("Press any key to continue");
            Console.ReadKey();

            cancellationTokenSource.Cancel();

            s.WaitWithoutException();
            c.WaitWithoutException();

            return;

            if (args == null || args.Length == 0)
            {
                var serverTask = RunServerAsync(options, cancellationTokenSource.Token);
                var clientTask1 = RunClientAsync("A", cancellationTokenSource.Token);
                var clientTask2 = RunClientAsync("B", cancellationTokenSource.Token);
                var clientTask3 = RunClientAsync("C", cancellationTokenSource.Token);

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
                var clientTask = RunClientAsync(args[1], cancellationTokenSource.Token);

                Console.WriteLine("Press any key to continue");
                Console.ReadKey();

                cancellationTokenSource.Cancel();

                clientTask.WaitWithoutException();
            }
        }

        static async Task RunServerAsync(ISmtpServerOptions options, CancellationToken cancellationToken)
        {
            var smtpServer = new SmtpServer.SmtpServer(options);

            smtpServer.SessionCreated += OnSmtpServerSessionCreated;
            smtpServer.SessionCompleted += OnSmtpServerSessionCompleted;

            await smtpServer.StartAsync(cancellationToken);

            smtpServer.SessionCreated -= OnSmtpServerSessionCreated;
            smtpServer.SessionCompleted -= OnSmtpServerSessionCompleted;
        }

        static async Task RunClientAsync(string name, CancellationToken cancellationToken)
        {
            var counter = 1;
            while (cancellationToken.IsCancellationRequested == false)
            {
                using (var smtpClient = new SmtpClient())
                {
                    await smtpClient.ConnectAsync("localhost", 9025, false, cancellationToken);
                    //smtpClient.EnableSsl = true;

                    try
                    {
                        var message = new MimeKit.MimeMessage();
                        message.From.Add(new MimeKit.MailboxAddress($"{name}{counter}@test.com"));
                        message.To.Add(new MimeKit.MailboxAddress("sample@test.com"));
                        message.Subject = $"{name} {counter}";

                        message.Body = new TextPart(TextFormat.Plain)
                        {
                            Text = "Assunto teste acento çãõáéíóú",
                            //Text = "Assunto teste acento",
                        };

                        await smtpClient.SendAsync(message, cancellationToken).ConfigureAwait(false);
                    }
                    catch (Exception exception)
                    {
                        Console.WriteLine(exception);
                    }

                    await smtpClient.DisconnectAsync(true, cancellationToken);
                }

                counter++;
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

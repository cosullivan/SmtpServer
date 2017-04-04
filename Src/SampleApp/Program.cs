using System;
using System.IO;
using System.Net.Mail;
using System.Net.Mime;
using System.Net.Security;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SmtpServer;
using SmtpServer.Tracing;
using MailKit.Net.Smtp;
using MimeKit;
using SmtpClient = MailKit.Net.Smtp.SmtpClient;

namespace SampleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            //var content = "Subject test çãõáéíóú";
            //var reader =new StreamReader(new MemoryStream(Encoding.UTF8.GetBytes(content)), Encoding.UTF8);
            //Console.WriteLine(reader.ReadToEnd());
            //return;


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
            //var c = RunClientAsync("A", cancellationTokenSource.Token);

            Console.WriteLine("Press any key to continue");
            Console.ReadKey();

            cancellationTokenSource.Cancel();

            s.WaitWithoutException();
            //c.WaitWithoutException();

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
                        //message.Subject = $"{name} {counter}";
                        message.Subject = "Assunto teste acento çãõáéíóú";
                        //message.Body = new MimeKit.TextPart("plain")
                        //{
                        //    Text = ""
                        //};
                        message.Body = new MimeKit.MimePart()
                        {
                            ContentTransferEncoding = MimeKit.ContentEncoding.EightBit
                        };

                        //var builder = new BodyBuilder();
                        //builder.Attachments.Add(
                        //    new MultipartAlternative() { ContentEncoding = ContentEncoding.EightBit});
                        //message.Prepare(MimeKit.EncodingConstraint.EightBit);
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

        //static async Task RunClientAsync(string name, CancellationToken cancellationToken)
        //{
        //    var counter = 1;
        //    while (cancellationToken.IsCancellationRequested == false)
        //    {
        //        var message = new MailMessage("test@gmail.com", "receiveaccount@test.com")
        //        {
        //            Subject = "Subject test çãõáéíóú",
        //            IsBodyHtml = false,
        //            Body = null
        //        };

        //        using (AlternateView body = AlternateView.CreateAlternateViewFromString(
        //            "Body special char çãõáéíóú",
        //            message.BodyEncoding,
        //            message.IsBodyHtml ? "text/html" : null))
        //        {
        //            message.SubjectEncoding = System.Text.Encoding.UTF7;
        //            body.TransferEncoding = TransferEncoding.EightBit;
        //            message.AlternateViews.Add(body);
        //            try
        //            {
        //                using (var smtp = new System.Net.Mail.SmtpClient("127.0.0.1", 9025))
        //                {
        //                    smtp.DeliveryFormat = SmtpDeliveryFormat.International;
        //                    smtp.EnableSsl = false;
        //                    smtp.Send(message);
        //                }
        //                Console.WriteLine("Sent with success.");
        //            }
        //            catch (SmtpException ex)
        //            {
        //                Console.WriteLine("Error on sent. Ex:" + ex.Message);
        //                System.Diagnostics.Debug.WriteLine(
        //                    ex.Message);
        //            }
        //        }

        //        counter++;
        //    }
        //}

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

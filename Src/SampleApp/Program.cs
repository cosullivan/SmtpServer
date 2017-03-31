using System;
using System.IO;
using System.Net.Security;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using SmtpServer;
using SmtpServer.Tracing;
using MailKit.Net.Smtp;
using System.Net.Mail;
using System.Net.Mime;

namespace SampleApp
{
    class Program
    {
        static void Main(string[] args)
        {
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

            if (args[0] == "8bit")
            {
                var serverTask = RunServerAsync(options, cancellationTokenSource.Token);
                var clientTask1 = RunClientAsync("A", cancellationTokenSource.Token);

                Console.WriteLine("Press any key to continue");
                Console.ReadKey();

                cancellationTokenSource.Cancel();

                serverTask.WaitWithoutException();
                clientTask1.WaitWithoutException();

                return;
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
                using (var smtpClient = new MailKit.Net.Smtp.SmtpClient())
                {
                    await smtpClient.ConnectAsync("localhost", 9025, false, cancellationToken);
                    //smtpClient.EnableSsl = true;

                    try
                    {
                        var message = new MimeKit.MimeMessage();
                        message.From.Add(new MimeKit.MailboxAddress($"{name}{counter}@test.com"));
                        message.To.Add(new MimeKit.MailboxAddress("sample@test.com"));
                        message.Subject = $"{name} {counter}";
                        message.Body = new MimeKit.TextPart("plain")
                        {
                            Text = ""
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

        static async Task RunClient8bitAsync(string name, CancellationToken cancellationToken)
        {
            var counter = 1;
            while (!cancellationToken.IsCancellationRequested)
            {
                var message = new MailMessage("ronaldo.lambda@gmail.com", "suporte@encontact.com.br")
                {
                    Subject = "Assunto teste acento çãõáéíóú",
                    IsBodyHtml = false,
                    Body = null
                };
                using (AlternateView body = AlternateView.CreateAlternateViewFromString(
                        "Assunto teste acento çãõáéíóú - Nro:" + counter,
                        message.BodyEncoding,
                        message.IsBodyHtml ? "text/html" : null))
                {
                    body.TransferEncoding = TransferEncoding.EightBit;
                    message.AlternateViews.Add(body);
                    try
                    {
                        using (var smtp = new System.Net.Mail.SmtpClient("localhost", 9025))
                        {
                            smtp.Send(message);
                        }
                        Console.WriteLine("E-mail enviado com sucesso.");
                    }
                    catch (SmtpException ex)
                    {
                        Console.WriteLine("Erro ao enviar email. Ex:" + ex.Message);
                        System.Diagnostics.Debug.WriteLine(
                            ex.Message);
                    }
                }
                //using (var smtpClient = new SmtpClient())
                //{
                //    await smtpClient.ConnectAsync("localhost", 9025, false, cancellationToken);
                //    try
                //    {
                //        var message = new MimeKit.MimeMessage();
                //        message.From.Add(new MimeKit.MailboxAddress($"{name}{counter}@test.com"));
                //        message.To.Add(new MimeKit.MailboxAddress("sample@test.com"));
                //        message.Subject = $"{name} {counter}";
                //        message.Body = new MimeKit.TextPart(MimeKit.Text.TextFormat.Plain)
                //        {
                //            Text = "áéíóú"
                //        };
                //        await smtpClient.SendAsync(message, cancellationToken).ConfigureAwait(false);
                //    }
                //    catch (Exception exception)
                //    {
                //        Console.WriteLine(exception);
                //    }

                //    await smtpClient.DisconnectAsync(true, cancellationToken);
                //}

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

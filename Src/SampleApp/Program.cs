using System;
using System.Net.Mail;
using System.Threading;
using System.Threading.Tasks;
using SmtpServer;

namespace SampleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            var cancellationTokenSource = new CancellationTokenSource();

            var options = new OptionsBuilder()
                .ServerName("SmtpServer SampleApp")
                .Port(9025)
                .MessageStore(new ConsoleMessageStore())
                .MailboxFilter(new ConsoleMailboxFilter())
                .Build();

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
                using (var smtpClient = new SmtpClient("localhost", 9025))
                {
                    try
                    {
                        await smtpClient.SendMailAsync(
                            new MailMessage("sample@test.com", "sample@test.com")
                            {
                                Subject = $"{name} {counter++}"
                            });
                    }
                    catch (SmtpException smtpException)
                    {
                        Console.WriteLine(smtpException.StatusCode);
                    }
                }

                await Task.Delay(250, cancellationToken);
            }
        }

        static void OnSmtpServerSessionCreated(object sender, SessionEventArgs sessionEventArgs)
        {
            Console.WriteLine("SessionCreated: {0}", sessionEventArgs.Context.RemoteEndPoint);
        }

        static void OnSmtpServerSessionCompleted(object sender, SessionEventArgs sessionEventArgs)
        {
            Console.WriteLine("SessionCompleted: {0}", sessionEventArgs.Context.RemoteEndPoint);
        }
    }
}

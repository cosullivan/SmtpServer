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
                using (var smtpClient = new SmtpClient("localhost", 9025))
                {
                    try
                    {
                        var message = new MailMessage($"{name}{counter}@test.com", "sample@test.com", $"{name} {counter}", "");
                        await Task.Run(() => smtpClient.Send(message), cancellationToken).ConfigureAwait(false);
                    }
                    catch (SmtpException smtpException)
                    {
                        Console.WriteLine(smtpException.StatusCode);
                    }
                    catch (Exception exception)
                    {
                        Console.WriteLine(exception);
                    }
                }

                if (counter % 1000 == 0)
                {
                    System.GC.Collect();
                }

                counter++;
                //await Task.Delay(50, cancellationToken).ConfigureAwait(false);
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

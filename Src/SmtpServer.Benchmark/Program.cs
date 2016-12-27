using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Limilabs.Client.SMTP;
using Limilabs.Mail;

namespace SmtpServer.Benchmark
{
    class Program
    {
        const string EnronCorpusFolder = @"C:\Dev\Enron Corpus";

        static void Main(string[] args)
        {
            var cancellationTokenSource = new CancellationTokenSource();

            var options = new OptionsBuilder()
                .ServerName("SmtpServer Benchmark")
                .Port(9025)
                .Build();

            var serverTask = RunServerAsync(cancellationTokenSource.Token);

            RunClient(EnronCorpusFolder);

            cancellationTokenSource.Cancel();
        }

        static async Task RunServerAsync(CancellationToken cancellationToken)
        {
            var options = new OptionsBuilder()
                .ServerName("SmtpServer Benchmark")
                .Port(9025)
                .Build();

            var smtpServer = new SmtpServer(options);

            await smtpServer.StartAsync(cancellationToken);
        }

        static void RunClient(string folder)
        {
            IReadOnlyList<IMail> messages;
            using (new ConsoleTimer("Creating Messages"))
            {
                messages = Directory.GetFiles(folder, "*.*", SearchOption.AllDirectories)
                    .Select(file => new Limilabs.Mail.MailBuilder().CreateFromEmlFile(file))
                    .Where(IsValid)
                    .ToList();
            }

            using (var smtpClient = new Smtp())
            {
                smtpClient.Connect("localhost", 9025);

                var timer = new ConsoleTimer("Sending Messages");
                using (timer)
                {
                    for (var i = 0; i < messages.Count; i++)
                    {
                        try
                        {
                            smtpClient.SendMessage(messages[i]);
                        }
                        catch (Exception exception)
                        {
                            Console.WriteLine(exception);
                        }

                        //if (i % 1000 == 0)
                        //{
                        //    Console.WriteLine((i + 1) / (messages.Count * 1.0));
                        //}
                    }
                }

                Console.WriteLine("Messages: {0}, Time: {1}ms, MPS: {2}", 
                    messages.Count, 
                    timer.TimeTaken, 
                    messages.Count / (timer.TimeTaken * 1.0) * 1000);
            }
        }

        static bool IsValid(IMail message)
        {
            if (message.To.Count == 0)
            {
                return false;
            }

            foreach (var toAddress in message.To)
            {
                foreach (var mailbox in toAddress.GetMailboxes())
                {
                    if (mailbox.LocalPart.Contains(".."))
                    {
                        return false;
                    }
                }
            }

            return true;
        }
    }
}
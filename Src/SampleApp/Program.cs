using SampleApp.Examples;

namespace SampleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            SecureServerExample.Run();
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

        //    await smtpServer.StartAsync(cancellationToken);
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
        //                    await smtpClient.ConnectAsync("localhost", 9025, false, cancellationToken);

        //                    if (smtpClient.Capabilities.HasFlag(SmtpCapabilities.Authentication))
        //                    {
        //                        await smtpClient.AuthenticateAsync("user", "password", cancellationToken);
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
        //                await smtpClient.DisconnectAsync(true, cancellationToken);
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
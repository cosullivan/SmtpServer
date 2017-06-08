using SampleApp.Examples;

namespace SampleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            //SimpleExample.Run();
            //SessionTracingExample.Run();
            SessionContextExample.Run();
        }

        //var cancellationTokenSource = new CancellationTokenSource();

        //    var certificate = CreateCertificate();

        //    ServicePointManager.ServerCertificateValidationCallback = IgnoreCertificateValidationFailureForTestingOnly;

        //    var options = new OptionsBuilder()
        //        .ServerName("SmtpServer SampleApp")
        //        .Port(9025)
        //        .Certificate(certificate)
        //        .SupportedSslProtocols(SslProtocols.Default)
        //        .MessageStore(new SampleMessageStore())
        //        //.MailboxFilter(new SampleMailboxFilter())
        //        .UserAuthenticator(new SampleUserAuthenticator())
        //        .Build();

        //    var s = RunServerAsync(options, cancellationTokenSource.Token);
        //    var c = RunClientAsync("A", 1, false, cancellationTokenSource.Token);
        //    //var c = RunFolderAsync(@"C:\Dev\temp\", "msg.txt", CancellationToken.None);
        //    //var c = RunFileAsync(@"c:\dev\temp\msg.txt", CancellationToken.None);

        //    Console.WriteLine("Press any key to continue");
        //    Console.ReadKey();

        //    cancellationTokenSource.Cancel();

        //    s.WaitWithoutException();
        //    c.WaitWithoutException();

        //    return;

        //    if (args == null || args.Length == 0)
        //    {
        //        var serverTask = RunServerAsync(options, cancellationTokenSource.Token);
        //        var clientTask1 = RunClientAsync("A", cancellationToken: cancellationTokenSource.Token);
        //        var clientTask2 = RunClientAsync("B", cancellationToken: cancellationTokenSource.Token);
        //        var clientTask3 = RunClientAsync("C", cancellationToken: cancellationTokenSource.Token);

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
        //        var serverTask = RunServerAsync(options, cancellationTokenSource.Token);

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

        //static async Task RunServerAsync(ISmtpServerOptions options, CancellationToken cancellationToken)
        //{
        //    var smtpServer = new SmtpServer.SmtpServer(options);

        //    smtpServer.SessionCreated += OnSmtpServerSessionCreated;
        //    smtpServer.SessionCompleted += OnSmtpServerSessionCompleted;

        //    await smtpServer.StartAsync(cancellationToken);

        //    smtpServer.SessionCreated -= OnSmtpServerSessionCreated;
        //    smtpServer.SessionCompleted -= OnSmtpServerSessionCompleted;
        //}

        //static async Task RunClientAsync(
        //    string name,
        //    int limit = Int32.MaxValue,
        //    bool forceConnection = true,
        //    CancellationToken cancellationToken = default(CancellationToken))
        //{
        //    var message = MimeKit.MimeMessage.Load(ParserOptions.Default, @"C:\Dev\Enron Corpus\maildir\allen-p\inbox\31_");
            
        //    var stopwatch = new Stopwatch();
        //    stopwatch.Start();

        //    using (var smtpClient = new SmtpClient())
        //    {
        //        var counter = 1;
        //        while (limit-- > 0 && cancellationToken.IsCancellationRequested == false)
        //        {
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

        //                //await SendMessageAsync(smtpClient, name, counter, cancellationToken);
        //                await smtpClient.SendAsync(message, cancellationToken).ConfigureAwait(false);
        //            }
        //            catch (Exception exception)
        //            {
        //                Console.WriteLine(exception);
        //            }

        //            if (forceConnection)
        //            {
        //                await smtpClient.DisconnectAsync(true, cancellationToken);
        //            }

        //            counter++;
        //        }
        //    }

        //    stopwatch.Stop();

        //    Console.WriteLine("Finished. Time Taken {0}ms", stopwatch.ElapsedMilliseconds);
        //}

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
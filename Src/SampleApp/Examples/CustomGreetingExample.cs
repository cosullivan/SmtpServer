//using System;
//using System.IO;
//using System.IO.Pipelines;
//using System.Security.Authentication;
//using System.Security.Cryptography.X509Certificates;
//using System.Threading;
//using System.Threading.Tasks;
//using SmtpServer;
//using SmtpServer.Authentication;
//using SmtpServer.ComponentModel;
//using SmtpServer.IO;
//using SmtpServer.Net;
//using SmtpServer.Protocol;
//using SmtpServer.Storage;
//using SmtpServer.Text;

//namespace SampleApp.Examples
//{
//    public static class CustomGreetingExample
//    {
//        public static void Run()
//        {
//            var cancellationTokenSource = new CancellationTokenSource();

//            var options = new SmtpServerOptionsBuilder()
//                .ServerName("SmtpServer SampleApp")
//                .Port(9025)
//                .Build();

//            var serviceProvider = new ServiceProvider();
//            serviceProvider.Add(new CustomSmtpCommandFactory());

//            var server = new SmtpServer.SmtpServer(options, serviceProvider);

//            var serverTask = server.StartAsync(cancellationTokenSource.Token);

//            SampleMailClient.Send(useSsl: true);

//            cancellationTokenSource.Cancel();
//            serverTask.WaitWithoutException();
//        }

//        public sealed class CustomSmtpCommandFactory : SmtpCommandFactory
//        {
//            public CustomSmtpCommandFactory(
//                ISmtpServerOptions options,
//                IUserAuthenticatorFactory userAuthenticatorFactory,
//                IMailboxFilterFactory mailboxFilterFactory,
//                IMessageStoreFactory messageStoreFactory) : base(options, userAuthenticatorFactory, mailboxFilterFactory, messageStoreFactory) { }

//            public override SmtpCommand CreateHelo(string domainOrAddress)
//            {
//                return new HeloCommand(domainOrAddress, "Good morning, Vietnam!");
//            }
//        }
//    }
//}
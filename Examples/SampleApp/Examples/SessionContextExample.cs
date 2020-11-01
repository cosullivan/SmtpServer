using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SmtpServer;
using SmtpServer.Authentication;
using SmtpServer.ComponentModel;
using SmtpServer.Protocol;
using SmtpServer.Tracing;

namespace SampleApp.Examples
{
    public static class SessionContextExample
    {
        static CancellationTokenSource _cancellationTokenSource;

        public static void Run()
        {
            _cancellationTokenSource = new CancellationTokenSource();

            var options = new SmtpServerOptionsBuilder()
                .ServerName("SmtpServer SampleApp")
                .Endpoint(builder =>
                    builder
                        .AllowUnsecureAuthentication()
                        .AuthenticationRequired()
                        .Port(9025))
                .Build();

            var serviceProvider = new ServiceProvider();
            serviceProvider.Add(new AuthenticationHandler());

            var server = new SmtpServer.SmtpServer(options, serviceProvider);

            server.SessionCreated += OnSessionCreated;
            server.SessionCompleted += OnSessionCompleted;

            var serverTask = server.StartAsync(_cancellationTokenSource.Token);

            SampleMailClient.Send(user: "cain", password: "o'sullivan", count: 5);

            serverTask.WaitWithoutException();
        }

        static void OnSessionCreated(object sender, SessionEventArgs e)
        {
            // the session context contains a Properties dictionary 
            // which can be used to custom session context

            e.Context.Properties["Start"] = DateTimeOffset.Now;
            e.Context.Properties["Commands"] = new List<SmtpCommand>();

            e.Context.CommandExecuting += OnCommandExecuting;
        }

        static void OnCommandExecuting(object sender, SmtpCommandEventArgs e)
        {
            ((List<SmtpCommand>)e.Context.Properties["Commands"]).Add(e.Command);
        }

        static void OnSessionCompleted(object sender, SessionEventArgs e)
        {
            e.Context.CommandExecuting -= OnCommandExecuting;

            Console.WriteLine("The session started at {0}.", e.Context.Properties["Start"]);
            Console.WriteLine();

            Console.WriteLine("The user that authenticated was {0}", e.Context.Properties["User"]);
            Console.WriteLine();

            Console.WriteLine("The following commands were executed during the session;");
            Console.WriteLine();

            var writer = new TracingSmtpCommandVisitor(Console.Out);

            foreach (var command in (List<SmtpCommand>)e.Context.Properties["Commands"])
            {
                writer.Visit(command);
            }

            _cancellationTokenSource.Cancel();
        }

        public class AuthenticationHandler : UserAuthenticator
        {
            public override Task<bool> AuthenticateAsync(
                ISessionContext context,
                string user,
                string password,
                CancellationToken cancellationToken)
            {
                context.Properties["User"] = user;

                return Task.FromResult(true);
            }
        }
    }
}
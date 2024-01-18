using System;
using System.Threading;
using MailKit;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using MimeKit.Text;

namespace SmtpServer.Tests
{
    internal static class MailClient
    {
        public static MimeMessage Message(
            string from = null,
            string to = null,
            string cc = null,
            string bcc = null,
            string subject = null,
            string text = null,
            string charset = "utf-8",
            MimeEntity body = null)
        {
            var message = new MimeMessage();

            message.From.Add(MailboxAddress.Parse(from ?? "from@sample.com"));
            message.To.Add(MailboxAddress.Parse(to ?? "to@sample.com"));

            if (cc != null)
            {
                message.Cc.Add(MailboxAddress.Parse(cc));
            }

            if (bcc != null)
            {
                message.Bcc.Add(MailboxAddress.Parse(bcc));
            }

            message.Subject = subject ?? "Hello";

            if (body == null)
            {
                body = new TextPart(TextFormat.Plain);
                ((TextPart)body).SetText(charset, text ?? "Hello World");
            }

            message.Body = body;

            return message;
        }

        public static SmtpClientEx Client()
        {
            return new SmtpClientEx();
        }

        public static SmtpClientEx Connect(this SmtpClientEx client,
            EventHandler<ConnectedEventArgs> onConnected = null,
            string host = "localhost",
            int port = 9025,
            SecureSocketOptions options = SecureSocketOptions.Auto)
        {
            if (onConnected is not null)
            {
                client.Connected += onConnected;
            }

            client.Connect(host, port, options);

            return client;
        }

        public static void Send(
            MimeMessage message = null,
            string user = null,
            string password = null)
        {
            message ??= Message();

            using var client = Client().Connect();

            if (user != null && password != null)
            {
                client.Authenticate(user, password);
            }

            //client.NoOp();

            client.Send(message);
            client.Disconnect(true);
        }

        public static void NoOp(SecureSocketOptions options = SecureSocketOptions.Auto)
        {
            using var client = Client().Connect(options: options);

            client.NoOp();
        }
    }

    internal class SmtpClientEx : SmtpClient
    {
        public SmtpResponse SendUnknownCommand(string command, CancellationToken cancellationToken = default)
        {
            return SendCommand(command, cancellationToken);
        }
    }
}
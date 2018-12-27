using MailKit.Net.Smtp;
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

            message.From.Add(new MailboxAddress(from ?? "from@sample.com"));
            message.To.Add(new MailboxAddress(to ?? "to@sample.com"));

            if (cc != null)
            {
                message.Cc.Add(new MailboxAddress(cc));
            }

            if (bcc != null)
            {
                message.Bcc.Add(new MailboxAddress(bcc));
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

        public static SmtpClient Client(string host = "localhost", int port = 9025)
        {
            var client = new SmtpClient();
            
            client.Connected += (sender, args) =>
            {

            };

            client.Connect("localhost", 9025);

            return client;
        }

        public static void Send(
            MimeMessage message = null,
            string user = null,
            string password = null)
        {
            message = message ?? Message();

            using (var client = Client())
            {
                if (user != null && password != null)
                {
                    client.Authenticate(user, password);
                }

                client.NoOp();

                client.Send(message);
                client.Disconnect(true);
            }
        }
    }
}
using MailKit.Net.Smtp;
using MimeKit;

namespace SmtpServer.Tests
{
    internal static class MailClient
    {
        public static void Send(
            string from = null,
            string to = null,
            string cc = null,
            string bcc = null,
            string subject = null,
            string user = null,
            string password = null,
            string text = null,
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

            message.Body = body ?? new TextPart("plain")
            {
                Text = text ?? "Hello World"
            };

            using (var client = new SmtpClient())
            {
                client.Connect("localhost", 9025);

                if (user != null && password != null)
                {
                    client.Authenticate(user, password);
                }

                client.Send(message);
                client.Disconnect(true);
            }
        }
    }
}
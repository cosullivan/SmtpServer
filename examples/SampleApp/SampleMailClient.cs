using System.Threading;
using MailKit.Net.Smtp;
using MimeKit;

namespace SampleApp
{
    public static class SampleMailClient
    {
        public static void Send(
            string from = null, 
            string to = null, 
            string subject = null,
            string user = null, 
            string password = null,
            MimeEntity body = null,
            int count = 1,
            int recipients = 1,
            bool useSsl = false,
            int port = 9025)
        {
            var message = new MimeMessage();

            message.From.Add(MailboxAddress.Parse(from ?? "from@sample.com"));
            
            for (var i = 0; i < recipients; i++)
            {
                message.To.Add(MailboxAddress.Parse(to ?? $"to_{i}@sample.com"));
            }

            message.Subject = subject ?? "Hello";
            message.Body = body ?? new TextPart("plain")
            {
                Text = "Hello World"
            };

            using var client = new SmtpClientEx();

            client.Connect("localhost", port, useSsl);

            if (user != null && password != null)
            {
                client.Authenticate(user, password);
            }

            client.SendUnknownCommand("ABCD EFGH IJKL");

            while (count-- > 0)
            {
                client.Send(message);
            }

            client.Disconnect(true);
        }

        internal class SmtpClientEx : SmtpClient
        {
            public SmtpResponse SendUnknownCommand(string command, CancellationToken cancellationToken = default)
            {
                return SendCommand(command, cancellationToken);
            }
        }
    }
}

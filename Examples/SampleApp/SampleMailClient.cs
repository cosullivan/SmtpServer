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
            bool useSsl = false,
            int port = 9025)
        {
            //var message = MimeMessage.Load(@"C:\Dev\Cain\Temp\message.eml");
            var message = new MimeMessage();

            message.From.Add(MailboxAddress.Parse(from ?? "from@sample.com"));
            message.To.Add(MailboxAddress.Parse(to ?? "to@sample.com"));
            //for (var i = 0; i < 400; i++)
            //{
            //    message.To.Add(MailboxAddress.Parse(to ?? $"testuser{i+1000}@longemaildomain1000001.com"));
            //}
            message.Subject = subject ?? "Hello";
            message.Body = body ?? new TextPart("plain")
            {
                Text = "Hello World"
            };

            using var client = new SmtpClient();

            client.Connect("localhost", port, useSsl);

            if (user != null && password != null)
            {
                client.Authenticate(user, password);
            }

            while (count-- > 0)
            {
                client.Send(message);
            }

            client.Disconnect(true);
        }
    }
}
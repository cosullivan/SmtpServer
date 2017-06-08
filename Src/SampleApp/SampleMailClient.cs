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
            MimeEntity body = null)
        {
            var message = new MimeMessage();

            message.From.Add(new MailboxAddress(from ?? "from@sample.com"));
            message.To.Add(new MailboxAddress(to ?? "to@sample.com"));
            message.Subject = subject ?? "Hello";
            message.Body = body ?? new TextPart("plain")
            {
                Text = "Hello World"
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
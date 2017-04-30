using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MimeKit;
using MimeKit.Text;
using SmtpServer;
using SmtpServer.Mail;
using SmtpServer.Protocol;
using SmtpServer.Storage;

namespace SampleApp
{
    public class ConsoleMessageStore : MessageStore
    {
        /// <summary>
        /// Save the given message to the underlying storage system.
        /// </summary>
        /// <param name="context">The session context.</param>
        /// <param name="message">The SMTP message to store.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A unique identifier that represents this message in the underlying message store.</returns>
        public override Task<SmtpResponse> SaveAsync(ISessionContext context, IMimeMessage message, CancellationToken cancellationToken)
        {
            //Console.WriteLine("From: {0} ({1})", message.From, context.RemoteEndPoint);
            //Console.WriteLine("To: {0}", String.Join(",", message.To.Select(m => m.AsAddress())));
            //Console.WriteLine(message.Mime);

            var m = MimeKit.MimeMessage.Load(new MemoryStream(System.Text.Encoding.UTF8.GetBytes(message.Mime.ToString())));

            Console.WriteLine(m.Subject);
            Console.WriteLine(m.TextBody);

            return Task.FromResult(SmtpResponse.Ok);
        }
    }
}
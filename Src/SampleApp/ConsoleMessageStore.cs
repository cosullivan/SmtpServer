using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SmtpServer;
using SmtpServer.Mail;
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
        public override Task<string> SaveAsync(ISessionContext context, IMimeMessage message, CancellationToken cancellationToken)
        {
            Console.WriteLine("From: {0} ({1})", message.From, context.RemoteEndPoint);
            Console.WriteLine("To: {0}", String.Join(",", message.To.Select(m => m.AsAddress())));
            Console.WriteLine(message.Mime);

            return Task.FromResult(Guid.NewGuid().ToString());
        }
    }
}
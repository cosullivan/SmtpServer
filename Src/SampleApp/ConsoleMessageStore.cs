using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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
        /// <param name="transaction">The SMTP message transaction to store.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A unique identifier that represents this message in the underlying message store.</returns>
        public override Task<SmtpResponse> SaveAsync(ISessionContext context, IMessageTransaction transaction, CancellationToken cancellationToken)
        {
            Console.WriteLine("From: {0} ({1})", transaction.From, context.RemoteEndPoint);
            Console.WriteLine("To: {0}", String.Join(",", transaction.To.Select(m => m.AsAddress())));

            Console.WriteLine(new StreamReader(((ITextMessage)transaction.Message).Content).ReadToEnd());

            return Task.FromResult(SmtpResponse.Ok);
        }
    }
}
using System;
using System.Threading;
using System.Threading.Tasks;
using SmtpServer;
using SmtpServer.Mail;
using SmtpServer.Protocol;
using SmtpServer.Storage;

namespace SampleApp
{
    public class SampleMessageStore : MessageStore
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
            //var textMessage = (ITextMessage)transaction.Message;

            //try
            //{
            //    var message = MimeKit.MimeMessage.Load(textMessage.Content);

            //    Console.WriteLine();
            //    Console.WriteLine("Subject: {0}", message.Subject);
            //    Console.WriteLine("Message:");
            //    Console.WriteLine(message.TextBody);
            //    Console.WriteLine();
            //}
            //catch (Exception e)
            //{
            //    Console.WriteLine(e);

            //    return Task.FromResult(SmtpResponse.TransactionFailed);
            //}

            var mimeMessage = (IMimeMessage) transaction.Message;

            Console.WriteLine(mimeMessage.Document.Version);
            
            return Task.FromResult(SmtpResponse.Ok);
        }
    }
}
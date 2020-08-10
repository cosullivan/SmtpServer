using System;
using System.Buffers;
using System.IO;
using System.Text;
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
        /// <param name="buffer">The buffer that contains the message content.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A unique identifier that represents this message in the underlying message store.</returns>
        public override Task<SmtpResponse> SaveAsync(ISessionContext context, IMessageTransaction transaction, ReadOnlySequence<byte> buffer, CancellationToken cancellationToken)
        {
            //var textMessage = (ITextMessage)transaction.Message;

            //using (var reader = new StreamReader(textMessage.Content, Encoding.UTF8))
            //{
            //    Console.WriteLine(reader.ReadToEnd());
            //}

            return Task.FromResult(SmtpResponse.Ok);
        }
    }
}
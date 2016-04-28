using System;
using System.Threading;
using System.Threading.Tasks;
using SmtpServer.Mail;
using SmtpServer.Storage;

namespace SampleApp
{
    public class ConsoleMessageStore : IMessageStore
    {
        /// <summary>
        /// Save the given message to the underlying storage system.
        /// </summary>
        /// <param name="message">The SMTP message to store.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A unique identifier that represents this message in the underlying message store.</returns>
        public Task<string> SaveAsync(IMimeMessage message, CancellationToken cancellationToken)
        {
            Console.WriteLine(message.Mime);

            return Task.FromResult(Guid.NewGuid().ToString());
        }
    }
}
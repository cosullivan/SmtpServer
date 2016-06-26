using System;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using SmtpServer.Mail;

namespace SmtpServer.Storage
{
    internal class DoNothingMessageStore : MessageStore
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
            return Task.FromResult(DateTimeOffset.Now.Ticks.ToString(CultureInfo.InvariantCulture));
        }
    }
}
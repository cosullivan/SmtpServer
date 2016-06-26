using System;
using System.Threading;
using System.Threading.Tasks;
using SmtpServer;
using SmtpServer.Mail;
using SmtpServer.Storage;

namespace SampleApp
{
    public class ConsoleMessageStore : MessageStore
    {
        readonly ISessionContext _context;

        /// <summary>
        /// Constructor.
        /// </summary>
        public ConsoleMessageStore() { }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="context">The session level context.</param>
        ConsoleMessageStore(ISessionContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Creates an instance of the message store specifically for this session.
        /// </summary>
        /// <param name="context">The session level context.</param>
        /// <returns>The message store instance specifically for this session.</returns>
        public override IMessageStore CreateSessionInstance(ISessionContext context)
        {
            return new ConsoleMessageStore(context);
        }

        /// <summary>
        /// Save the given message to the underlying storage system.
        /// </summary>
        /// <param name="message">The SMTP message to store.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A unique identifier that represents this message in the underlying message store.</returns>
        public override Task<string> SaveAsync(IMimeMessage message, CancellationToken cancellationToken)
        {
            //Console.WriteLine("From: {0} ({1})", message.From, _context.RemoteEndPoint);
            //Console.WriteLine("To: {0}", String.Join(",", message.To.Select(m => m.AsAddress())));
            //Console.WriteLine(message.Mime);

            Console.WriteLine(message.From.User);

            return Task.FromResult(Guid.NewGuid().ToString());
        }
    }
}
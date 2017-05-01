using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SmtpServer.IO;

namespace SmtpServer.Protocol
{
    public sealed class EhloCommand : SmtpCommand
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="options">The server options.</param>
        /// <param name="domainOrAddress">The domain name or address literal.</param>
        internal EhloCommand(ISmtpServerOptions options, string domainOrAddress) : base(options)
        {
            DomainOrAddress = domainOrAddress;
        }

        /// <summary>
        /// Execute the command.
        /// </summary>
        /// <param name="context">The execution context to operate on.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task which asynchronously performs the execution.</returns>
        internal override async Task ExecuteAsync(ISmtpSessionContext context, CancellationToken cancellationToken)
        {
            var greeting = $"{Options.ServerName} Hello {DomainOrAddress}, haven't we met before?";
            var output = new[] { greeting }.Union(GetExtensions(context)).ToArray();

            for (var i = 0; i < output.Length - 1; i++)
            {
                await context.Text.WriteLineAsync($"250-{output[i]}", cancellationToken);
            }

            await context.Text.WriteLineAsync($"250 {output[output.Length - 1]}", cancellationToken);
            await context.Text.FlushAsync(cancellationToken);
        }

        /// <summary>
        /// Gets the list of extensions.
        /// </summary>
        /// <param name="session">The session the is currently operating.</param>
        /// <returns>The list of extensions that are allowed for the session.</returns>
        IEnumerable<string> GetExtensions(ISmtpSessionContext session)
        {
            yield return "PIPELINING";
            yield return "8BITMIME";

            if (session.Text.IsSecure == false && Options.ServerCertificate != null)
            {
                yield return "STARTTLS";
            }

            if (Options.MaxMessageSize > 0)
            {
                yield return $"SIZE {Options.MaxMessageSize}";
            }

            if (IsPlainLoginAllowed(session))
            {
                yield return "AUTH PLAIN LOGIN";
            }
        }

        /// <summary>
        /// Returns a value indicating whether or not plain login is allowed.
        /// </summary>
        /// <param name="session">The current session.</param>
        /// <returns>true if plain login is allowed for the session, false if not.</returns>
        bool IsPlainLoginAllowed(ISmtpSessionContext session)
        {
            if (Options.UserAuthenticator == null)
            {
                return false;
            }

            return session.Text.IsSecure || Options.AllowUnsecureAuthentication;
        }

        /// <summary>
        /// Gets the domain name or address literal.
        /// </summary>
        public string DomainOrAddress { get; }
    }
}
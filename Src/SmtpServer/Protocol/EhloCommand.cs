using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SmtpServer.IO;

namespace SmtpServer.Protocol
{
    public sealed class EhloCommand : SmtpCommand
    {
        public const string Command = "EHLO";

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="options">The server options.</param>
        /// <param name="domainOrAddress">The domain name or address literal.</param>
        internal EhloCommand(ISmtpServerOptions options, string domainOrAddress) : base(Command, options)
        {
            DomainOrAddress = domainOrAddress;
        }

        /// <summary>
        /// Execute the command.
        /// </summary>
        /// <param name="context">The execution context to operate on.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Returns true if the command executed successfully such that the transition to the next state should occurr, false 
        /// if the current state is to be maintained.</returns>
        internal override async Task<bool> ExecuteAsync(SmtpSessionContext context, CancellationToken cancellationToken)
        {
            var greeting = $"{Options.ServerName} Hello {DomainOrAddress}, haven't we met before?";
            
            var output = new[] { greeting }.Union(GetExtensions(context)).ToArray();

            for (var i = 0; i < output.Length - 1; i++)
            {
                context.Pipe.Output.WriteLine($"250-{output[i]}");
            }

            context.Pipe.Output.WriteLine($"250 {output[output.Length - 1]}");

            await context.Pipe.Output.FlushAsync(cancellationToken).ConfigureAwait(false);

            return true;
        }

        /// <summary>
        /// Gets the list of extensions.
        /// </summary>
        /// <param name="context">The session context the is currently operating.</param>
        /// <returns>The list of extensions that are allowed for the session.</returns>
        IEnumerable<string> GetExtensions(ISessionContext context)
        {
            yield return "PIPELINING";
            yield return "8BITMIME";
            yield return "SMTPUTF8";

            if (context.Pipe.IsSecure == false && Options.ServerCertificate != null)
            {
                yield return "STARTTLS";
            }

            if (Options.MaxMessageSize > 0)
            {
                yield return $"SIZE {Options.MaxMessageSize}";
            }

            if (IsPlainLoginAllowed(context))
            {
                yield return "AUTH PLAIN LOGIN";
            }
        }

        /// <summary>
        /// Returns a value indicating whether or not plain login is allowed.
        /// </summary>
        /// <param name="context">The current session context.</param>
        /// <returns>true if plain login is allowed for the session, false if not.</returns>
        bool IsPlainLoginAllowed(ISessionContext context)
        {
            if (Options.UserAuthenticatorFactory == null)
            {
                return false;
            }

            return context.Pipe.IsSecure || context.EndpointDefinition.AllowUnsecureAuthentication;
        }

        /// <summary>
        /// Gets the domain name or address literal.
        /// </summary>
        public string DomainOrAddress { get; }
    }
}
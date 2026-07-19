using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SmtpServer.Authentication;
using SmtpServer.IO;

namespace SmtpServer.Protocol
{
    /// <summary>
    /// Ehlo Command
    /// </summary>
    public class EhloCommand : SmtpCommand
    {
        /// <summary>
        /// Smtp Ehlo Command
        /// </summary>
        public const string Command = "EHLO";

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="domainOrAddress">The domain name or address literal.</param>
        public EhloCommand(string domainOrAddress) : base(Command)
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
            if (await AcceptHeloAsync(context, cancellationToken).ConfigureAwait(false) == false)
            {
                return false;
            }

            var greeting = GetGreeting(context);

            using (var extensions = GetExtensions(context).GetEnumerator())
            {
                if (extensions.MoveNext() == false)
                {
                    context.Pipe.Output.WriteLine($"250 {greeting}");
                }
                else
                {
                    context.Pipe.Output.WriteLine($"250-{greeting}");

                    var extension = extensions.Current;
                    while (extensions.MoveNext())
                    {
                        context.Pipe.Output.WriteLine($"250-{extension}");
                        extension = extensions.Current;
                    }

                    context.Pipe.Output.WriteLine($"250 {extension}");
                }
            }

            await context.Pipe.Output.FlushAsync(cancellationToken).ConfigureAwait(false);

            return true;
        }

        /// <summary>
        /// Returns the greeting to display to the remote host.
        /// </summary>
        /// <param name="context">The session context.</param>
        /// <returns>The greeting text to display to the remote host.</returns>
        protected virtual string GetGreeting(ISessionContext context)
        {
            return $"{context.ServerOptions.ServerName} Hello {DomainOrAddress}, haven't we met before?";
        }

        /// <summary>
        /// Returns the list of extensions that are current for the context.
        /// </summary>
        /// <param name="context">The session context.</param>
        /// <returns>The list of extensions that are current for the context.</returns>
        protected virtual IEnumerable<string> GetExtensions(ISessionContext context)
        {
            yield return "PIPELINING";
            yield return "8BITMIME";

            if (context.ServerOptions.Extensions.SmtpUtf8Enabled)
            {
                yield return "SMTPUTF8";
            }

            if (context.ServerOptions.Extensions.DsnEnabled)
            {
                yield return "DSN";
            }

            if (context.ServerOptions.Extensions.ChunkingEnabled)
            {
                yield return "CHUNKING";
            }

            if (context.Pipe.IsSecure == false && context.EndpointDefinition.CertificateFactory != null)
            {
                yield return "STARTTLS";
            }

            if (context.ServerOptions.MaxMessageSizeOptions.Length > 0)
            {
                yield return $"SIZE {context.ServerOptions.MaxMessageSizeOptions.Length}";
            }

            if (IsPlainLoginAllowed(context))
            {
                var mechanisms = "AUTH PLAIN LOGIN";

                // Advertise the bearer-token mechanisms only when the host opts in (it has wired an
                // authenticator that can validate a token), so a client never negotiates one we cannot honour.
                if (context.ServerOptions.Extensions.OAuthEnabled)
                {
                    mechanisms += " XOAUTH2 OAUTHBEARER";
                }

                yield return mechanisms;
            }

            static bool IsPlainLoginAllowed(ISessionContext context)
            {
                if (context.ServiceProvider.GetService(typeof(IUserAuthenticatorFactory)) == null && context.ServiceProvider.GetService(typeof(IUserAuthenticator)) == null)
                {
                    return false;
                }

                return context.Pipe.IsSecure || context.EndpointDefinition.AllowUnsecureAuthentication;
            }
        }

        async Task<bool> AcceptHeloAsync(SmtpSessionContext context, CancellationToken cancellationToken)
        {
            var policy = context.ServerOptions.SessionPolicy;
            if (policy.Helo == null)
            {
                return true;
            }

            var response = await policy.Helo(context, DomainOrAddress, cancellationToken).ConfigureAwait(false);
            if (SmtpSession.IsSuccessResponse(response))
            {
                return true;
            }

            await context.Pipe.Output.WriteReplyAsync(response, cancellationToken).ConfigureAwait(false);
            return false;
        }

        /// <summary>
        /// Gets the domain name or address literal.
        /// </summary>
        public string DomainOrAddress { get; }
    }
}

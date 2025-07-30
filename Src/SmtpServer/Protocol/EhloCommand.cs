using System.Collections.Generic;
using System.Linq;
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
            var output = new[] { GetGreeting(context) }.Union(GetExtensions(context)).ToArray();

            for (var i = 0; i < output.Length - 1; i++)
            {
                context.Pipe.Output.WriteLine($"250-{output[i]}");
            }

            context.Pipe.Output.WriteLine($"250 {output[output.Length - 1]}");

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
            yield return "SMTPUTF8";

            if (context.Pipe.IsSecure == false && context.EndpointDefinition.CertificateFactory != null)
            {
                yield return "STARTTLS";
            }

            if (context.ServerOptions.MaxMessageSize > 0)
            {
                yield return $"SIZE {context.ServerOptions.MaxMessageSize}";
            }

            var supportedLoginTypes = new List<string>();
            if (IsPlainLoginAllowed(context))
            {
                supportedLoginTypes.Add("PLAIN");
                supportedLoginTypes.Add("LOGIN");
            }

            if (IsBearerTokenLoginAllowed(context))
            {
                supportedLoginTypes.Add("XOAUTH2");
                supportedLoginTypes.Add("OAUTHBEARER");
            }

            if (supportedLoginTypes.Count > 0)
            {
                yield return $"AUTH {string.Join(" ", supportedLoginTypes)}";
            }

            static bool IsPlainLoginAllowed(ISessionContext context)
            {
                if (context.ServiceProvider.GetService(typeof(IUserAuthenticatorFactory)) == null && context.ServiceProvider.GetService(typeof(IUserAuthenticator)) == null)
                {
                    return false;
                }

                return context.Pipe.IsSecure || context.EndpointDefinition.AllowUnsecureAuthentication;
            }

            static bool IsBearerTokenLoginAllowed(ISessionContext context)
            {
                if (context.ServiceProvider.GetService(typeof(IBearerTokenAuthenticatorFactory)) == null
                    && context.ServiceProvider.GetService(typeof(IBearerTokenAuthenticator)) == null)
                {
                    return false;
                }

                return context.Pipe.IsSecure || context.EndpointDefinition.AllowUnsecureAuthentication;
            }
        }

        /// <summary>
        /// Gets the domain name or address literal.
        /// </summary>
        public string DomainOrAddress { get; }
    }
}

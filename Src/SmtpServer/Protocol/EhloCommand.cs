﻿using System.Collections.Generic;
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
        internal EhloCommand(ISmtpServerOptions options, string domainOrAddress) : base(options)
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
                await context.NetworkClient.WriteLineAsync($"250-{output[i]}", cancellationToken).ConfigureAwait(false);
            }

            await context.NetworkClient.WriteLineAsync($"250 {output[output.Length - 1]}", cancellationToken).ConfigureAwait(false);
            await context.NetworkClient.FlushAsync(cancellationToken).ConfigureAwait(false);

            return true;
        }

        /// <summary>
        /// Gets the list of extensions.
        /// </summary>
        /// <param name="session">The session the is currently operating.</param>
        /// <returns>The list of extensions that are allowed for the session.</returns>
        IEnumerable<string> GetExtensions(SmtpSessionContext session)
        {
            yield return "PIPELINING";
            yield return "8BITMIME";
            yield return "SMTPUTF8";

            if (session.NetworkClient.IsSecure == false && Options.ServerCertificate != null)
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
        bool IsPlainLoginAllowed(SmtpSessionContext session)
        {
            if (Options.UserAuthenticatorFactory == null)
            {
                return false;
            }

            return session.NetworkClient.IsSecure || session.EndpointDefinition.AllowUnsecureAuthentication;
        }

        /// <summary>
        /// Gets the domain name or address literal.
        /// </summary>
        public string DomainOrAddress { get; }
    }
}
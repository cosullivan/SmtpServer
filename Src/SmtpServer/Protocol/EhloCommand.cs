﻿using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SmtpServer.Protocol
{
    public sealed class EhloCommand : SmtpCommand
    {
        readonly string _domainOrAddress;
        readonly ISmtpServerOptions _options;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="domainOrAddress">The domain name or address literal.</param>
        /// <param name="options">The list of server options.</param>
        public EhloCommand(string domainOrAddress, ISmtpServerOptions options)
        {
            _domainOrAddress = domainOrAddress;
            _options = options;
        }

        /// <summary>
        /// Execute the command.
        /// </summary>
        /// <param name="context">The execution context to operate on.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task which asynchronously performs the execution.</returns>
        public override async Task ExecuteAsync(ISmtpSessionContext context, CancellationToken cancellationToken)
        {
            var greeting = $"{_options.ServerName} Hello {DomainOrAddress}, haven't we met before?";
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

            if (!session.Text.IsSecure && _options.ServerCertificate != null)
            {
                yield return "STARTTLS";
            }

            if (_options.MaxMessageSize > 0)
            {
                yield return $"SIZE {_options.MaxMessageSize}";
            }

            if (IsPlainLoginAllowed(session))
            {
                yield return "AUTH PLAIN LOGIN";
            }

            if(session.Text.TransferEncodeType == Text.TransferEncodeType.EightBitMime)
            {
                yield return "8BITMIME";
            }
        }

        /// <summary>
        /// Returns a value indicating whether or not plain login is allowed.
        /// </summary>
        /// <param name="session">The current session.</param>
        /// <returns>true if plain login is allowed for the session, false if not.</returns>
        bool IsPlainLoginAllowed(ISmtpSessionContext session)
        {
            if (_options.UserAuthenticator == null)
            {
                return false;
            }

            return session.Text.IsSecure || _options.AllowUnsecureAuthentication;
        }

        /// <summary>
        /// Gets the domain name or address literal.
        /// </summary>
        public string DomainOrAddress
        {
            get { return _domainOrAddress; }
        }
    }
}
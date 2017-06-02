using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using SmtpServer.IO;

namespace SmtpServer.Protocol
{
    public class AuthCommand : SmtpCommand
    {
        public const string Command = "AUTH";

        string _user;
        string _password;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="options">The server options.</param>
        /// <param name="method">The authentication method.</param>
        /// <param name="parameter">The authentication parameter.</param>
        internal AuthCommand(ISmtpServerOptions options, AuthenticationMethod method, string parameter) : base(options)
        {
            Method = method;
            Parameter = parameter;
        }

        /// <summary>
        /// Execute the command.
        /// </summary>
        /// <param name="context">The execution context to operate on.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task which asynchronously performs the execution.</returns>
        internal override async Task ExecuteAsync(SmtpSessionContext context, CancellationToken cancellationToken)
        {
            switch (Method)
            {
                case AuthenticationMethod.Plain:
                    if (await TryPlainAsync(context, cancellationToken).ReturnOnAnyThread() == false)
                    {
                        await context.Client.ReplyAsync(SmtpResponse.AuthenticationFailed, cancellationToken).ReturnOnAnyThread();
                        return;
                    }
                    break;

                case AuthenticationMethod.Login:
                    if (await TryLoginAsync(context, cancellationToken).ReturnOnAnyThread() == false)
                    {
                        await context.Client.ReplyAsync(SmtpResponse.AuthenticationFailed, cancellationToken).ReturnOnAnyThread();
                        return;
                    }
                    break;
            }

            if (await Options.UserAuthenticator.AuthenticateAsync(_user, _password).ReturnOnAnyThread() == false)
            {
                await context.Client.ReplyAsync(SmtpResponse.AuthenticationFailed, cancellationToken).ReturnOnAnyThread();
                return;
            }

            await context.Client.ReplyAsync(SmtpResponse.AuthenticationSuccessful, cancellationToken).ReturnOnAnyThread();

            context.RaiseSessionAuthenticated();
        }

        /// <summary>
        /// Attempt a PLAIN login sequence.
        /// </summary>
        /// <param name="context">The execution context to operate on.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>true if the PLAIN login sequence worked, false if not.</returns>
        async Task<bool> TryPlainAsync(SmtpSessionContext context, CancellationToken cancellationToken)
        {
            await context.Client.ReplyAsync(new SmtpResponse(SmtpReplyCode.ContinueWithAuth, " "), cancellationToken).ConfigureAwait(false);

            var authentication = await ReadBase64EncodedLineAsync(context.Client, cancellationToken).ReturnOnAnyThread();

            var match = Regex.Match(authentication, "\x0000(?<user>.*)\x0000(?<password>.*)");

            if (match.Success == false)
            {
                await context.Client.ReplyAsync(SmtpResponse.AuthenticationFailed, cancellationToken).ConfigureAwait(false);
                return false;
            }

            _user = match.Groups["user"].Value;
            _password = match.Groups["password"].Value;

            return true;
        }

        /// <summary>
        /// Attempt a LOGIN login sequence.
        /// </summary>
        /// <param name="context">The execution context to operate on.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>true if the LOGIN login sequence worked, false if not.</returns>
        async Task<bool> TryLoginAsync(SmtpSessionContext context, CancellationToken cancellationToken)
        {
            await context.Client.ReplyAsync(new SmtpResponse(SmtpReplyCode.ContinueWithAuth, "VXNlcm5hbWU6"), cancellationToken);

            _user = await ReadBase64EncodedLineAsync(context.Client, cancellationToken).ReturnOnAnyThread();
          
            await context.Client.ReplyAsync(new SmtpResponse(SmtpReplyCode.ContinueWithAuth, "UGFzc3dvcmQ6"), cancellationToken);

            _password = await ReadBase64EncodedLineAsync(context.Client, cancellationToken).ReturnOnAnyThread();

            return true;
        }

        /// <summary>
        /// Read a Base64 encoded line.
        /// </summary>
        /// <param name="client">The client to read from.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The decoded Base64 string.</returns>
        async Task<string> ReadBase64EncodedLineAsync(INetworkClient client, CancellationToken cancellationToken)
        {
            var text = await client.ReadLineAsync(Encoding.ASCII, cancellationToken).ReturnOnAnyThread();

            return Encoding.UTF8.GetString(Convert.FromBase64String(text));
        }

        /// <summary>
        /// The authentication method.
        /// </summary>
        public AuthenticationMethod Method { get; }

        /// <summary>
        /// The athentication parameter.
        /// </summary>
        public string Parameter { get; }
    }
}
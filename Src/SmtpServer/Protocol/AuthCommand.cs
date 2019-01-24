using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using SmtpServer.Authentication;
using SmtpServer.IO;
using SmtpServer.Storage;

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
        /// <returns>Returns true if the command executed successfully such that the transition to the next state should occurr, false 
        /// if the current state is to be maintained.</returns>
        internal override async Task<bool> ExecuteAsync(SmtpSessionContext context, CancellationToken cancellationToken)
        {
            context.IsAuthenticated = false;

            switch (Method)
            {
                case AuthenticationMethod.Plain:
                    if (await TryPlainAsync(context, cancellationToken).ReturnOnAnyThread() == false)
                    {
                        await context.NetworkClient.ReplyAsync(SmtpResponse.AuthenticationFailed, cancellationToken).ReturnOnAnyThread();
                        return false;
                    }
                    break;

                case AuthenticationMethod.Login:
                    if (await TryLoginAsync(context, cancellationToken).ReturnOnAnyThread() == false)
                    {
                        await context.NetworkClient.ReplyAsync(SmtpResponse.AuthenticationFailed, cancellationToken).ReturnOnAnyThread();
                        return false;
                    }
                    break;
            }

            using (var container = new DisposableContainer<IUserAuthenticator>(Options.UserAuthenticatorFactory.CreateInstance(context)))
            {
                if (await container.Instance.AuthenticateAsync(context, _user, _password, cancellationToken).ReturnOnAnyThread() == false)
                {
                    await context.NetworkClient.ReplyAsync(SmtpResponse.AuthenticationFailed, cancellationToken).ReturnOnAnyThread();
                    return false;
                }
            }

            await context.NetworkClient.ReplyAsync(SmtpResponse.AuthenticationSuccessful, cancellationToken).ReturnOnAnyThread();

            context.IsAuthenticated = true;
            context.AuthenticatedUser = _user;
            context.RaiseSessionAuthenticated();

            return true;
        }

        /// <summary>
        /// Attempt a PLAIN login sequence.
        /// </summary>
        /// <param name="context">The execution context to operate on.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>true if the PLAIN login sequence worked, false if not.</returns>
        async Task<bool> TryPlainAsync(SmtpSessionContext context, CancellationToken cancellationToken)
        {
            var authentication = Parameter;

            if (String.IsNullOrWhiteSpace(authentication))
            { 
                await context.NetworkClient.ReplyAsync(new SmtpResponse(SmtpReplyCode.ContinueWithAuth, " "), cancellationToken).ReturnOnAnyThread();

                authentication = await context.NetworkClient.ReadLineAsync(Encoding.ASCII, cancellationToken).ReturnOnAnyThread();
            }

            if (TryExtractFromBase64(authentication) == false)
            {
                await context.NetworkClient.ReplyAsync(SmtpResponse.AuthenticationFailed, cancellationToken).ConfigureAwait(false);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Attempt to extract the user name and password combination from a single line base64 encoded string.
        /// </summary>
        /// <param name="base64">The base64 encoded string to extract the user name and password from.</param>
        /// <returns>true if the user name and password were extracted from the base64 encoded string, false if not.</returns>
        bool TryExtractFromBase64(string base64)
        {
            var match = Regex.Match(Encoding.UTF8.GetString(Convert.FromBase64String(base64)), "\x0000(?<user>.*)\x0000(?<password>.*)");

            if (match.Success == false)
            {
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
            if (String.IsNullOrWhiteSpace(Parameter) == false)
            {
                _user = Encoding.UTF8.GetString(Convert.FromBase64String(Parameter));
            }
            else
            {
                await context.NetworkClient.ReplyAsync(new SmtpResponse(SmtpReplyCode.ContinueWithAuth, "VXNlcm5hbWU6"), cancellationToken);

                _user = await ReadBase64EncodedLineAsync(context.NetworkClient, cancellationToken).ReturnOnAnyThread();
            }
          
            await context.NetworkClient.ReplyAsync(new SmtpResponse(SmtpReplyCode.ContinueWithAuth, "UGFzc3dvcmQ6"), cancellationToken);

            _password = await ReadBase64EncodedLineAsync(context.NetworkClient, cancellationToken).ReturnOnAnyThread();

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

            return text == null 
                ? String.Empty 
                : Encoding.UTF8.GetString(Convert.FromBase64String(text));
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
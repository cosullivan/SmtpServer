using System;
using System.IO.Pipelines;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using SmtpServer.Authentication;
using SmtpServer.ComponentModel;
using SmtpServer.IO;

namespace SmtpServer.Protocol
{
    /// <summary>
    /// Auth Command
    /// </summary>
    public sealed class AuthCommand : SmtpCommand
    {
        /// <summary>
        /// Smtp Auth Command
        /// </summary>
        public const string Command = "AUTH";

        string _user;
        string _password;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="method">The authentication method.</param>
        /// <param name="parameter">The authentication parameter.</param>
        public AuthCommand(AuthenticationMethod method, string parameter) : base(Command)
        {
            Method = method;
            Parameter = parameter;
        }

        /// <summary>
        /// Execute the command.
        /// </summary>
        /// <param name="context">The execution context to operate on.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Returns true if the command executed successfully such that the transition to the next state should occur, false 
        /// if the current state is to be maintained.</returns>
        internal override async Task<bool> ExecuteAsync(SmtpSessionContext context, CancellationToken cancellationToken)
        {
            context.Authentication = AuthenticationContext.Unauthenticated;

            switch (Method)
            {
                case AuthenticationMethod.Plain:
                    if (await TryPlainAsync(context, cancellationToken).ConfigureAwait(false) == false)
                    {
                        await context.Pipe.Output.WriteReplyAsync(SmtpResponse.AuthenticationFailed, cancellationToken).ConfigureAwait(false);
                        return false;
                    }
                    break;

                case AuthenticationMethod.Login:
                    if (await TryLoginAsync(context, cancellationToken).ConfigureAwait(false) == false)
                    {
                        await context.Pipe.Output.WriteReplyAsync(SmtpResponse.AuthenticationFailed, cancellationToken).ConfigureAwait(false);
                        return false;
                    }
                    break;
            }

            var userAuthenticator = context.ServiceProvider.GetService<IUserAuthenticatorFactory, IUserAuthenticator>(context, UserAuthenticator.Default);

            using (var container = new DisposableContainer<IUserAuthenticator>(userAuthenticator))
            {
                if (await container.Instance.AuthenticateAsync(context, _user, _password, cancellationToken).ConfigureAwait(false) == false)
                {
                    var remaining = context.ServerOptions.MaxAuthenticationAttempts - ++context.AuthenticationAttempts;
                    var response = new SmtpResponse(SmtpReplyCode.AuthenticationFailed, $"authentication failed, {remaining} attempt(s) remaining.");

                    await context.Pipe.Output.WriteReplyAsync(response, cancellationToken).ConfigureAwait(false);

                    if (remaining <= 0)
                    {
                        throw new SmtpResponseException(SmtpResponse.ServiceClosingTransmissionChannel, true);
                    }

                    return false;
                }
            }

            await context.Pipe.Output.WriteReplyAsync(SmtpResponse.AuthenticationSuccessful, cancellationToken).ConfigureAwait(false);

            context.Authentication = new AuthenticationContext(_user);
            context.RaiseSessionAuthenticated();

            return true;
        }

        /// <summary>
        /// Attempt a PLAIN login sequence.
        /// </summary>
        /// <param name="context">The execution context to operate on.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>true if the PLAIN login sequence worked, false if not.</returns>
        async Task<bool> TryPlainAsync(ISessionContext context, CancellationToken cancellationToken)
        {
            var authentication = Parameter;

            if (string.IsNullOrWhiteSpace(authentication))
            {
                await context.Pipe.Output.WriteReplyAsync(new SmtpResponse(SmtpReplyCode.ContinueWithAuth, " "), cancellationToken).ConfigureAwait(false);

                authentication = await context.Pipe.Input.ReadLineAsync(Encoding.ASCII, context.ServerOptions.MaxMessageSizeOptions, cancellationToken).ConfigureAwait(false);
            }

            if (TryExtractFromBase64(authentication) == false)
            {
                await context.Pipe.Output.WriteReplyAsync(SmtpResponse.AuthenticationFailed, cancellationToken).ConfigureAwait(false);
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
        async Task<bool> TryLoginAsync(ISessionContext context, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(Parameter) == false)
            {
                _user = Encoding.UTF8.GetString(Convert.FromBase64String(Parameter));
            }
            else
            {
                //Username = VXNlcm5hbWU6 (base64)
                await context.Pipe.Output.WriteReplyAsync(new SmtpResponse(SmtpReplyCode.ContinueWithAuth, "VXNlcm5hbWU6"), cancellationToken).ConfigureAwait(false);

                _user = await ReadBase64EncodedLineAsync(context.Pipe.Input, context.ServerOptions.MaxMessageSizeOptions, cancellationToken).ConfigureAwait(false);
            }

            //Password = UGFzc3dvcmQ6 (base64)
            await context.Pipe.Output.WriteReplyAsync(new SmtpResponse(SmtpReplyCode.ContinueWithAuth, "UGFzc3dvcmQ6"), cancellationToken).ConfigureAwait(false);

            _password = await ReadBase64EncodedLineAsync(context.Pipe.Input, context.ServerOptions.MaxMessageSizeOptions, cancellationToken).ConfigureAwait(false);

            return true;
        }

        /// <summary>
        /// Read a Base64 encoded line.
        /// </summary>
        /// <param name="reader">The pipe to read from.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The decoded Base64 string.</returns>
        static async Task<string> ReadBase64EncodedLineAsync(PipeReader reader, IMaxMessageSizeOptions maxMessageSizeOptions, CancellationToken cancellationToken)
        {
            var text = await reader.ReadLineAsync(maxMessageSizeOptions, cancellationToken);

            return text == null ? string.Empty : Encoding.UTF8.GetString(Convert.FromBase64String(text));
        }

        /// <summary>
        /// The authentication method.
        /// </summary>
        public AuthenticationMethod Method { get; }

        /// <summary>
        /// The authentication parameter.
        /// </summary>
        public string Parameter { get; }
    }
}

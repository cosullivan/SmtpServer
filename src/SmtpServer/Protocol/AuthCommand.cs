using System;
using System.IO.Pipelines;
using System.Text;
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

                case AuthenticationMethod.XOAuth2:
                    if (await TryOAuthAsync(context, xOAuth2: true, cancellationToken).ConfigureAwait(false) == false)
                    {
                        await context.Pipe.Output.WriteReplyAsync(SmtpResponse.AuthenticationFailed, cancellationToken).ConfigureAwait(false);
                        return false;
                    }
                    break;

                case AuthenticationMethod.OAuthBearer:
                    if (await TryOAuthAsync(context, xOAuth2: false, cancellationToken).ConfigureAwait(false) == false)
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

                authentication = await context.Pipe.Input.ReadLineAsync(Encoding.ASCII, context.ServerOptions.MaxCommandLineLength, cancellationToken).ConfigureAwait(false);
            }

            if (TryExtractFromBase64(authentication) == false)
            {
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
            if (TryDecodeBase64(base64, out var buffer, out var bytesWritten) == false)
            {
                return false;
            }

            try
            {
                var decoded = new ReadOnlySpan<byte>(buffer, 0, bytesWritten);

                var userStart = decoded.IndexOf((byte)0);
                if (userStart < 0)
                {
                    return false;
                }

                var passwordStart = decoded.Slice(userStart + 1).IndexOf((byte)0);
                if (passwordStart < 0)
                {
                    return false;
                }

                passwordStart += userStart + 1;

                _user = Encoding.UTF8.GetString(buffer, userStart + 1, passwordStart - userStart - 1);
                _password = Encoding.UTF8.GetString(buffer, passwordStart + 1, bytesWritten - passwordStart - 1);
            }
            finally
            {
                Array.Clear(buffer, 0, bytesWritten);
            }

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
                if (TryDecodeBase64String(Parameter, out _user) == false)
                {
                    return false;
                }
            }
            else
            {
                //Username = VXNlcm5hbWU6 (base64)
                await context.Pipe.Output.WriteReplyAsync(new SmtpResponse(SmtpReplyCode.ContinueWithAuth, "VXNlcm5hbWU6"), cancellationToken).ConfigureAwait(false);

                _user = await ReadBase64EncodedLineAsync(context.Pipe.Input, context.ServerOptions.MaxCommandLineLength, cancellationToken).ConfigureAwait(false);
                if (_user == null)
                {
                    return false;
                }
            }

            //Password = UGFzc3dvcmQ6 (base64)
            await context.Pipe.Output.WriteReplyAsync(new SmtpResponse(SmtpReplyCode.ContinueWithAuth, "UGFzc3dvcmQ6"), cancellationToken).ConfigureAwait(false);

            _password = await ReadBase64EncodedLineAsync(context.Pipe.Input, context.ServerOptions.MaxCommandLineLength, cancellationToken).ConfigureAwait(false);
            if (_password == null)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Attempt an XOAUTH2 or OAUTHBEARER bearer-token exchange. The decoded bearer token is surfaced to
        /// the authenticator as the password and the SASL identity as the user, so a host validates the token
        /// exactly as it validates any other credential — no live identity-provider call happens here.
        /// </summary>
        /// <param name="context">The execution context to operate on.</param>
        /// <param name="xOAuth2">true to decode the XOAUTH2 blob, false to decode the OAUTHBEARER blob.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>true if a user and bearer token were decoded, false if not.</returns>
        async Task<bool> TryOAuthAsync(ISessionContext context, bool xOAuth2, CancellationToken cancellationToken)
        {
            var response = Parameter;

            if (string.IsNullOrWhiteSpace(response))
            {
                await context.Pipe.Output.WriteReplyAsync(new SmtpResponse(SmtpReplyCode.ContinueWithAuth, " "), cancellationToken).ConfigureAwait(false);

                response = await context.Pipe.Input.ReadLineAsync(Encoding.ASCII, context.ServerOptions.MaxCommandLineLength, cancellationToken).ConfigureAwait(false);
            }

            return xOAuth2
                ? OAuthSaslDecoder.TryDecodeXOAuth2(response, out _user, out _password)
                : OAuthSaslDecoder.TryDecodeOAuthBearer(response, out _user, out _password);
        }

        /// <summary>
        /// Read a Base64 encoded line.
        /// </summary>
        /// <param name="reader">The pipe to read from.</param>
        /// <param name="maxLineLength">The maximum command line length.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The decoded Base64 string, or null when the line is invalid.</returns>
        static async Task<string> ReadBase64EncodedLineAsync(PipeReader reader, int maxLineLength, CancellationToken cancellationToken)
        {
            var text = await reader.ReadLineAsync(maxLineLength, cancellationToken);

            return TryDecodeBase64String(text, out var value) ? value : null;
        }

        static bool TryDecodeBase64String(string text, out string value)
        {
            value = null;

            if (TryDecodeBase64(text, out var buffer, out var bytesWritten) == false)
            {
                return false;
            }

            try
            {
                value = Encoding.UTF8.GetString(buffer, 0, bytesWritten);
            }
            finally
            {
                Array.Clear(buffer, 0, bytesWritten);
            }

            return true;
        }

        static bool TryDecodeBase64(string text, out byte[] buffer, out int bytesWritten)
        {
            buffer = null;
            bytesWritten = 0;

            if (string.IsNullOrWhiteSpace(text))
            {
                return false;
            }

            var maxByteCount = text.Length;
            buffer = new byte[maxByteCount];

            if (Convert.TryFromBase64String(text, buffer, out bytesWritten) == false)
            {
                Array.Clear(buffer, 0, buffer.Length);
                buffer = null;
                return false;
            }

            return true;
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

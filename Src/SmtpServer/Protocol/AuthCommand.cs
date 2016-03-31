using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using SmtpServer.Authentication;

namespace SmtpServer.Protocol
{
    public class AuthCommand : SmtpCommand
    {
        readonly IUserAuthenticator _userAuthenticator;
        readonly AuthenticationMethod _method;
        readonly string _parameter;
        string _user;
        string _password;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="userAuthenticator">The user authenticator.</param>
        /// <param name="method">The authentication method.</param>
        /// <param name="parameter">The authentication parameter.</param>
        public AuthCommand(IUserAuthenticator userAuthenticator, AuthenticationMethod method, string parameter)
        {
            _userAuthenticator = userAuthenticator;
            _method = method;
            _parameter = parameter;
        }

        /// <summary>
        /// Execute the command.
        /// </summary>
        /// <param name="context">The execution context to operate on.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task which asynchronously performs the execution.</returns>
        public override async Task ExecuteAsync(ISmtpSessionContext context, CancellationToken cancellationToken)
        {
            switch (_method)
            {
                case AuthenticationMethod.Plain:
                    if (await TryPlainAsync(context, cancellationToken) == false)
                    {
                        await context.Text.ReplyAsync(SmtpResponse.AuthenticationFailed, cancellationToken).ConfigureAwait(false);
                        return;
                    }
                    break;

                case AuthenticationMethod.Login:
                    if (await TryLoginAsync(context, cancellationToken) == false)
                    {
                        await context.Text.ReplyAsync(SmtpResponse.AuthenticationFailed, cancellationToken).ConfigureAwait(false);
                        return;
                    }
                    break;
            }

            if (await _userAuthenticator.AuthenticateAsync(_user, _password).ConfigureAwait(false) == false)
            {
                await context.Text.ReplyAsync(SmtpResponse.AuthenticationFailed, cancellationToken).ConfigureAwait(false);
                return;
            }

            await context.Text.ReplyAsync(SmtpResponse.AuthenticationSuccessful, cancellationToken).ConfigureAwait(false);

            context.StateMachine.RemoveCommand(SmtpState.WaitingForMail, "AUTH");
            context.StateMachine.RemoveCommand(SmtpState.WaitingForMailSecure, "AUTH");
        }

        /// <summary>
        /// Attempt a PLAIN login sequence.
        /// </summary>
        /// <param name="context">The execution context to operate on.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>true if the PLAIN login sequence worked, false if not.</returns>
        async Task<bool> TryPlainAsync(ISmtpSessionContext context, CancellationToken cancellationToken)
        {
            await context.Text.ReplyAsync(new SmtpResponse(SmtpReplyCode.ContinueWithAuth, " "), cancellationToken).ConfigureAwait(false);

            var authentication = Encoding.UTF8.GetString(
                Convert.FromBase64String(
                    await context.Text.ReadLineAsync(cancellationToken).ConfigureAwait(false)));

            var match = Regex.Match(authentication, "\x0000(?<user>.*)\x0000(?<password>.*)");

            if (match.Success == false)
            {
                await context.Text.ReplyAsync(SmtpResponse.AuthenticationFailed, cancellationToken).ConfigureAwait(false);
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
        async Task<bool> TryLoginAsync(ISmtpSessionContext context, CancellationToken cancellationToken)
        {
            await context.Text.ReplyAsync(new SmtpResponse(SmtpReplyCode.ContinueWithAuth, "VXNlcm5hbWU6"), cancellationToken);

            _user = Encoding.UTF8.GetString(
                Convert.FromBase64String(
                    await context.Text.ReadLineAsync(cancellationToken).ConfigureAwait(false)));

            await context.Text.ReplyAsync(new SmtpResponse(SmtpReplyCode.ContinueWithAuth, "UGFzc3dvcmQ6"), cancellationToken);

            _password = Encoding.UTF8.GetString(
                Convert.FromBase64String(
                    await context.Text.ReadLineAsync(cancellationToken).ConfigureAwait(false)));

            return true;
        }
    }
}

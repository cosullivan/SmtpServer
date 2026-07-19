using System.Threading;
using System.Threading.Tasks;

namespace SmtpServer.Protocol
{
    /// <summary>
    /// Default SMTP command policy.
    /// </summary>
    public abstract class SmtpCommandPolicy : ISmtpCommandPolicy
    {
        /// <summary>
        /// Default SMTP command policy.
        /// </summary>
        public static readonly ISmtpCommandPolicy Default = new DefaultSmtpCommandPolicy();

        /// <inheritdoc />
        public virtual Task<SmtpResponse> GetHelpAsync(ISessionContext context, string argument, CancellationToken cancellationToken)
        {
            var message = string.IsNullOrWhiteSpace(argument)
                ? "Commands: EHLO HELO MAIL RCPT DATA RSET NOOP QUIT STARTTLS AUTH HELP VRFY EXPN"
                : $"No additional help is available for {argument.Trim().ToUpperInvariant()}";

            return Task.FromResult(new SmtpResponse(SmtpReplyCode.HelpResponse, message));
        }

        /// <inheritdoc />
        public virtual Task<SmtpResponse> VerifyAsync(ISessionContext context, string argument, CancellationToken cancellationToken)
        {
            return Task.FromResult(new SmtpResponse(
                SmtpReplyCode.CantVerifyUser,
                "cannot VRFY user, but will accept message and attempt delivery"));
        }

        /// <inheritdoc />
        public virtual Task<SmtpResponse> ExpandAsync(ISessionContext context, string argument, CancellationToken cancellationToken)
        {
            return Task.FromResult(new SmtpResponse(SmtpReplyCode.CantVerifyUser, "cannot EXPN mailing list"));
        }

        sealed class DefaultSmtpCommandPolicy : SmtpCommandPolicy
        {
        }
    }
}

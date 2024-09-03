using System.Threading;
using System.Threading.Tasks;

namespace SmtpServer.Authentication
{
    /// <summary>
    /// User Authenticator Interface
    /// </summary>
    public interface IUserAuthenticator
    {
        /// <summary>
        /// Authenticate a user account.
        /// </summary>
        /// <param name="context">The session context.</param>
        /// <param name="user">The user to authenticate.</param>
        /// <param name="password">The password of the user.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>true if the user is authenticated, false if not.</returns>
        Task<bool> AuthenticateAsync(ISessionContext context, string user, string password, CancellationToken cancellationToken);
    }
}

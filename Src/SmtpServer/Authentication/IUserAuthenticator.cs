using System.Threading.Tasks;

namespace SmtpServer.Authentication
{
    public interface IUserAuthenticator
    {
        /// <summary>
        /// Authenticate a user account.
        /// </summary>
        /// <param name="user">The user to authenticate.</param>
        /// <param name="password">The password of the user.</param>
        /// <returns>true if the user is authenticated, false if not.</returns>
        Task<bool> AuthenticateAsync(string user, string password);
    }
}

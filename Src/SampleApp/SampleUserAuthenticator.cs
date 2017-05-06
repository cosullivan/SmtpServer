using System.Threading.Tasks;
using SmtpServer.Authentication;

namespace SampleApp
{
    public sealed class SampleUserAuthenticator : IUserAuthenticator
    {
        /// <summary>
        /// Authenticate a user account.
        /// </summary>
        /// <param name="user">The user to authenticate.</param>
        /// <param name="password">The password of the user.</param>
        /// <returns>true if the user is authenticated, false if not.</returns>
        public Task<bool> AuthenticateAsync(string user, string password)
        {
            return Task.FromResult(user == "user" && password == "password");
        }
    }
}
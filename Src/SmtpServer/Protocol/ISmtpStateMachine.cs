using SmtpServer.Text;

namespace SmtpServer.Protocol
{
    public interface ISmtpStateMachine
    {
        /// <summary>
        /// Advances the enumerator to the next command in the stream.
        /// </summary>
        /// <param name="tokenEnumerator">The token enumerator to accept the command from.</param>
        /// <param name="command">The command that was found.</param>
        /// <param name="errorResponse">The error response that indicates why a command could not be accepted.</param>
        /// <returns>true if a valid command was found, false if not.</returns>
        bool TryAccept(TokenEnumerator2 tokenEnumerator, out SmtpCommand command, out SmtpResponse errorResponse);

        /// <summary>
        /// Remove the specified command from the state.
        /// </summary>
        /// <param name="state">The SMTP state to remove the command from.</param>
        /// <param name="command">The command to remove from the state.</param>
        void RemoveCommand(SmtpState state, string command);
    }
}
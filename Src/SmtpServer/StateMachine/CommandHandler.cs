using SmtpServer.Protocol;

namespace SmtpServer.StateMachine
{
    public abstract class CommandHandler
    {
        /// <summary>
        /// Try to make the command.
        /// </summary>
        /// <param name="parser">The parser to create the command from.</param>
        /// <param name="command">The command that was made.</param>
        /// <param name="errorResponse">The error response to display if the command could not be made.</param>
        /// <returns>true if the command could be made, false if not.</returns>
        public abstract bool TryMake(SmtpParser parser, out SmtpCommand command, out SmtpResponse errorResponse);

        /// <summary>
        /// Returns the name of the command.
        /// </summary>
        public abstract string Name { get; }
    }
}
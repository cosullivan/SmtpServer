namespace SmtpServer.Protocol
{
    public interface ISmtpStateMachine
    {
        /// <summary>
        /// Remove the specified command from the state.
        /// </summary>
        /// <param name="state">The SMTP state to remove the command from.</param>
        /// <param name="command">The command to remove from the state.</param>
        void RemoveCommand(SmtpState state, string command);
    }
}

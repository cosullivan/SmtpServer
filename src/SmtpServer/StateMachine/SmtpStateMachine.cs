using SmtpServer.Protocol;
using System.Linq;

namespace SmtpServer.StateMachine
{
    internal sealed class SmtpStateMachine 
    {
        readonly SmtpSessionContext _context;
        SmtpState _state;
        SmtpStateTransition _transition;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="context">The SMTP server session context.</param>
        internal SmtpStateMachine(SmtpSessionContext context)
        {
            _state = SmtpStateTable.Shared[SmtpStateId.Initialized];
            _context = context;
        }

        /// <summary>
        /// Try to accept the command given the current state.
        /// </summary>
        /// <param name="command">The command to accept.</param>
        /// <param name="errorResponse">The error response to display if the command was not accepted.</param>
        /// <returns>true if the command could be accepted, false if not.</returns>
        public bool TryAccept(SmtpCommand command, out SmtpResponse errorResponse)
        {
            errorResponse = null;

            if (_state.Transitions.TryGetValue(command.Name, out var transition) == false || transition.CanAccept(_context) == false)
            {
                var commands = _state.Transitions.Where(t => t.Value.CanAccept(_context)).Select(t => t.Key);

                errorResponse = new SmtpResponse(SmtpReplyCode.SyntaxError, $"expected {string.Join("/", commands)}");
                return false;
            }

            _transition = transition;
            return true;
        }
        
        /// <summary>
        /// Accept the state and transition to the new state.
        /// </summary>
        /// <param name="context">The session context to use for accepting session based transitions.</param>
        public void Transition(SmtpSessionContext context)
        {
            _state = SmtpStateTable.Shared[_transition.Transition(context)];
        }
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using SmtpServer.Protocol;

namespace SmtpServer.StateMachine
{
    internal sealed class SmtpStateTable : IEnumerable
    {
        // TODO: this is the definition and should be made shareable amongst all sessions 

        internal static readonly SmtpStateTable Shared = new SmtpStateTable
        {
            new SmtpState(SmtpStateId.Initialized)
            {
                { NoopCommand.Command },
                { RsetCommand.Command },
                { QuitCommand.Command },
                { ProxyCommand.Command },
                { HeloCommand.Command, TransitionToWaitingForMailSecureWhenSecure },
                { EhloCommand.Command, TransitionToWaitingForMailSecureWhenSecure }
            }
        };

        static SmtpStateId TransitionToWaitingForMailSecureWhenSecure(SmtpSessionContext context)
        {
            return context.Pipe.IsSecure ? SmtpStateId.WaitingForMailSecure : SmtpStateId.WaitingForMail;
        }

        readonly IDictionary<SmtpStateId, SmtpState> _states = new Dictionary<SmtpStateId, SmtpState>();

        internal SmtpState this[SmtpStateId stateId] => _states[stateId];

        /// <summary>
        /// Add the state to the table.
        /// </summary>
        /// <param name="state"></param>
        void Add(SmtpState state)
        {
            _states.Add(state.StateId, state);
        }

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>An <see cref="T:System.Collections.IEnumerator"></see> object that can be used to iterate through the collection.</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            // this is just here for the collection initializer syntax to work
            throw new NotImplementedException();
        }
    }
}
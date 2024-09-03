using System;
using System.Collections;
using System.Collections.Generic;

namespace SmtpServer.StateMachine
{
    internal sealed class SmtpState : IEnumerable
    {
        internal SmtpState(SmtpStateId stateId)
        {
            StateId = stateId;
        }

        internal void Add(string command)
        {
            Transitions.Add(command, new SmtpStateTransition(context => true, context => StateId));
        }

        internal void Add(string command, SmtpStateId state)
        {
            Transitions.Add(command, new SmtpStateTransition(context => true, context => state));
        }

        internal void Add(string command, Func<SmtpSessionContext, SmtpStateId> transitionDelegate)
        {
            Transitions.Add(command, new SmtpStateTransition(context => true, transitionDelegate));
        }

        internal void Add(string command, Func<SmtpSessionContext, bool> canAcceptDelegate)
        {
            Transitions.Add(command, new SmtpStateTransition(canAcceptDelegate, context => StateId));
        }

        internal void Add(string command, Func<SmtpSessionContext, bool> canAcceptDelegate, SmtpStateId state)
        {
            Transitions.Add(command, new SmtpStateTransition(canAcceptDelegate, context => state));
        }

        internal void Add(string command, Func<SmtpSessionContext, bool> canAcceptDelegate, Func<SmtpSessionContext, SmtpStateId> transitionDelegate)
        {
            Transitions.Add(command, new SmtpStateTransition(canAcceptDelegate, transitionDelegate));
        }

        // this is just here for the collection initializer syntax to work
        IEnumerator IEnumerable.GetEnumerator() => throw new NotImplementedException();

        internal SmtpStateId StateId { get; }

        internal IDictionary<string, SmtpStateTransition> Transitions { get; } = new Dictionary<string, SmtpStateTransition>(StringComparer.OrdinalIgnoreCase);
    }
}

using System;
using System.Buffers;
using System.Collections;
using System.Collections.Generic;
using SmtpServer.Protocol;

namespace SmtpServer.StateMachine
{
    internal class SmtpStateMachine 
    {
        //delegate bool TryMakeDelegate(SmtpParser parser, out SmtpCommand command, out SmtpResponse errorResponse);

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
            //_context.SessionAuthenticated += OnSessionAuthenticated;
            //_stateTable = new StateTable
            //{
            //    new State(SmtpStateId.Initialized)
            //    {
            //        //{ EhloCommand.Command, EhloCommand.TryMake, c => c.Pipe.IsSecure ? SmtpState.WaitingForMailSecure : SmtpState.WaitingForMail },
            //        //{ EhloCommand.Command }
            //    }
            //    //new State(SmtpState.Initialized)
            //    //{
            //    //    { NoopCommand.Command, TryMakeNoop },
            //    //    { RsetCommand.Command, TryMakeRset },
            //    //    { QuitCommand.Command, TryMakeQuit },
            //    //    { ProxyCommand.Command, TryMakeProxy },
            //    //    { HeloCommand.Command, TryMakeHelo, c => c.Pipe.IsSecure ? SmtpState.WaitingForMailSecure : SmtpState.WaitingForMail },
            //    //    { EhloCommand.Command, TryMakeEhlo, c => c.Pipe.IsSecure ? SmtpState.WaitingForMailSecure : SmtpState.WaitingForMail },
            //    //},
            //    //new State(SmtpState.WaitingForMail)
            //    //{
            //    //    { NoopCommand.Command, TryMakeNoop },
            //    //    { RsetCommand.Command, TryMakeRset },
            //    //    { QuitCommand.Command, TryMakeQuit },
            //    //    { HeloCommand.Command, TryMakeHelo, SmtpState.WaitingForMail },
            //    //    { EhloCommand.Command, TryMakeEhlo, SmtpState.WaitingForMail },
            //    //    { MailCommand.Command, TryMakeMail, SmtpState.WithinTransaction }
            //    //},
            //    //new State(SmtpState.WaitingForMailSecure)
            //    //{
            //    //    { NoopCommand.Command, TryMakeNoop },
            //    //    { RsetCommand.Command, TryMakeRset },
            //    //    { QuitCommand.Command, TryMakeQuit },
            //    //    { AuthCommand.Command, TryMakeAuth },
            //    //    { HeloCommand.Command, TryMakeHelo, SmtpState.WaitingForMailSecure },
            //    //    { EhloCommand.Command, TryMakeEhlo, SmtpState.WaitingForMailSecure },
            //    //    { MailCommand.Command, TryMakeMail, SmtpState.WithinTransaction }
            //    //},
            //    //new State(SmtpState.WithinTransaction)
            //    //{
            //    //    { NoopCommand.Command, TryMakeNoop },
            //    //    { RsetCommand.Command, TryMakeRset, c => c.Pipe.IsSecure ? SmtpState.WaitingForMailSecure : SmtpState.WaitingForMail },
            //    //    { QuitCommand.Command, TryMakeQuit },
            //    //    { RcptCommand.Command, TryMakeRcpt, SmtpState.CanAcceptData },
            //    //},
            //    //new State(SmtpState.CanAcceptData)
            //    //{
            //    //    { NoopCommand.Command, TryMakeNoop },
            //    //    { RsetCommand.Command, TryMakeRset, c => c.Pipe.IsSecure ? SmtpState.WaitingForMailSecure : SmtpState.WaitingForMail },
            //    //    { QuitCommand.Command, TryMakeQuit },
            //    //    { RcptCommand.Command, TryMakeRcpt },
            //    //    { DataCommand.Command, TryMakeData, SmtpState.WaitingForMail },
            //    //}
            //};

            //if (context.EndpointDefinition.AllowUnsecureAuthentication)
            //{
            //    WaitingForMail.Add(AuthCommand.Command, TryMakeAuth);
            //}

            //if (context.EndpointDefinition.AuthenticationRequired)
            //{
            //    WaitingForMail.Replace(MailCommand.Command, MakeResponse(SmtpResponse.AuthenticationRequired));
            //    WaitingForMailSecure.Replace(MailCommand.Command, MakeResponse(SmtpResponse.AuthenticationRequired));
            //}

            //if (context.ServerOptions.ServerCertificate != null && context.Pipe.IsSecure == false)
            //{
            //    WaitingForMail.Add(StartTlsCommand.Command, TryMakeStartTls, SmtpState.WaitingForMailSecure);
            //}

            //_stateTable.Initialize(SmtpStateId.Initialized);
        }

        ///// <summary>
        ///// Called when the session has been authenticated.
        ///// </summary>
        ///// <param name="sender">The object that raised the event.</param>
        ///// <param name="eventArgs">The event data.</param>
        //void OnSessionAuthenticated(object sender, EventArgs eventArgs)
        //{
        //    // TODO: probably dont need this anymore as it can be handled elsewhere

        //    //_context.SessionAuthenticated -= OnSessionAuthenticated;

        //    //WaitingForMail.Remove(AuthCommand.Command);
        //    //WaitingForMailSecure.Remove(AuthCommand.Command);

        //    //if (_context.EndpointDefinition.AuthenticationRequired)
        //    //{
        //    //    WaitingForMail.Replace(MailCommand.Command, TryMakeMail, SmtpState.WithinTransaction);
        //    //    WaitingForMailSecure.Replace(MailCommand.Command, TryMakeMail, SmtpState.WithinTransaction);
        //    //}
        //}

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
                errorResponse = new SmtpResponse(SmtpReplyCode.SyntaxError, $"expected {string.Join("/", _state.Transitions.Keys)}");
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

        ///// <summary>
        ///// Returns the waiting for mail state.
        ///// </summary>
        //State WaitingForMail => _stateTable[SmtpState.WaitingForMail];

        ///// <summary>
        ///// Returns the waiting for mail in a secure transaction state.
        ///// </summary>
        //State WaitingForMailSecure => _stateTable[SmtpState.WaitingForMailSecure];

        //#region StateTable

        //class StateTable : IEnumerable
        //{
        //    readonly Dictionary<SmtpStateId, State> _states = new Dictionary<SmtpStateId, State>();
        //    SmtpStateId _current;
        //    StateTransition _transition;

        //    /// <summary>
        //    /// Sets the initial state.
        //    /// </summary>
        //    /// <param name="stateId">The ID of the initial state.</param>
        //    public void Initialize(SmtpStateId stateId)
        //    {
        //        _current = stateId;
        //    }

        //    /// <summary>
        //    /// Returns the state with the given ID.
        //    /// </summary>
        //    /// <param name="stateId">The state ID to return.</param>
        //    /// <returns>The state with the given id.</returns>
        //    public State this[SmtpStateId stateId] => _states[stateId];

        //    /// <summary>
        //    /// Add the given state.
        //    /// </summary>
        //    /// <param name="state"></param>
        //    public void Add(State state)
        //    {
        //        _states.Add(state.StateId, state);
        //    }

        //    ///// <summary>
        //    ///// Advances the enumerator to the next command in the stream.
        //    ///// </summary>
        //    ///// <param name="context">The session context to use for making session based transitions.</param>
        //    ///// <param name="tokenEnumerator">The token enumerator to accept the command from.</param>
        //    ///// <param name="command">The command that is defined within the token enumerator.</param>
        //    ///// <param name="errorResponse">The error that indicates why the command could not be made.</param>
        //    ///// <returns>true if a valid command was found, false if not.</returns>
        //    //public bool TryMake(SmtpSessionContext context, TokenEnumerator tokenEnumerator, out SmtpCommand command, out SmtpResponse errorResponse)
        //    //{
        //    //    if (_states[_current].Transitions.TryGetValue(tokenEnumerator.Peek().Text, out _transition) == false)
        //    //    {
        //    //        var response = $"expected {string.Join("/", _states[_current].Transitions.Keys)}";

        //    //        command = null;
        //    //        errorResponse = new SmtpResponse(SmtpReplyCode.SyntaxError, response);

        //    //        return false;
        //    //    }

        //    //    if (_transition.Delegate(tokenEnumerator, out command, out errorResponse) == false)
        //    //    {
        //    //        return false;
        //    //    }

        //    //    return true;
        //    //}

        //    /// <summary>
        //    /// Accept the state and transition to the new state.
        //    /// </summary>
        //    /// <param name="context">The session context to use for accepting session based transitions.</param>
        //    public void Transition(SmtpSessionContext context)
        //    {
        //        _current = _transition.Transition(context);
        //    }

        //    /// <summary>
        //    /// Returns an enumerator that iterates through a collection.
        //    /// </summary>
        //    /// <returns>An <see cref="T:System.Collections.IEnumerator"/> object that can be used to iterate through the collection.</returns>
        //    IEnumerator IEnumerable.GetEnumerator()
        //    {
        //        // this is just here for the collection initializer syntax to work
        //        throw new NotImplementedException();
        //    }
        //}

        //#endregion

        //#region State

        //class State : IEnumerable
        //{
        //    /// <summary>
        //    /// Constructor.
        //    /// </summary>
        //    /// <param name="stateId">The ID of the state.</param>
        //    public State(SmtpStateId stateId)
        //    {
        //        StateId = stateId;
        //        Transitions = new Dictionary<string, StateTransition>(StringComparer.OrdinalIgnoreCase);
        //    }

        //    ///// <summary>
        //    ///// Add a state action.
        //    ///// </summary>
        //    ///// <param name="command">The name of the SMTP command.</param>
        //    ///// <param name="tryMake">The function callback to create the command.</param>
        //    //public void Add(string command, TryMakeDelegate tryMake)
        //    //{
        //    //    Add(command, tryMake, context => StateId);
        //    //}

        //    ///// <summary>
        //    ///// Add a state action.
        //    ///// </summary>
        //    ///// <param name="command">The name of the SMTP command.</param>
        //    ///// <param name="tryMake">The function callback to create the command.</param>
        //    ///// <param name="transition">The state to transition to.</param>
        //    //public void Add(string command, SmtpState transition)
        //    //{
        //    //    Add(command, tryMake, context => transition);
        //    //}

        //    ///// <summary>
        //    ///// Add a state action.
        //    ///// </summary>
        //    ///// <param name="command">The name of the SMTP command.</param>
        //    ///// <param name="tryMake">The function callback to create the command.</param>
        //    ///// <param name="transition">The function to determine the new state.</param>
        //    //public void Add(string command, Func<SmtpSessionContext, SmtpState> transition)
        //    //{
        //    //    Transitions.Add(command, new StateTransition(tryMake, transition));
        //    //}

        //    ///// <summary>
        //    ///// Add a state action.
        //    ///// </summary>
        //    ///// <param name="command">The name of the SMTP command.</param>
        //    ///// <param name="tryMake">The function callback to create the command.</param>
        //    ///// <param name="transitionTo">The state to transition to.</param>
        //    //public void Replace(string command, SmtpState? transitionTo = null)
        //    //{
        //    //    Remove(command);
        //    //    Add(command, tryMake, transitionTo ?? StateId);
        //    //}

        //    ///// <summary>
        //    ///// Clear the command from the current state.
        //    ///// </summary>
        //    ///// <param name="command">The command to clear.</param>
        //    //public void Remove(string command)
        //    //{
        //    //    Transitions.Remove(command);
        //    //}

        //    /// <summary>
        //    /// Returns an enumerator that iterates through a collection.
        //    /// </summary>
        //    /// <returns>An <see cref="T:System.Collections.IEnumerator"/> object that can be used to iterate through the collection.</returns>
        //    IEnumerator IEnumerable.GetEnumerator()
        //    {
        //        // this is just here for the collection initializer syntax to work
        //        throw new NotImplementedException();
        //    }

        //    /// <summary>
        //    /// Gets ID of the state.
        //    /// </summary>
        //    public SmtpStateId StateId { get; }

        //    /// <summary>
        //    /// Gets the actions that are available to the state.
        //    /// </summary>
        //    public Dictionary<string, StateTransition> Transitions { get; }
        //}

        //#endregion

        //#region StateTransition

        //class StateTransition
        //{
        //    /// <summary>
        //    /// Constructor.
        //    /// </summary>
        //    /// <param name="transition">The transition function to move from the previous state to the new state.</param>
        //    public StateTransition(Func<SmtpSessionContext, SmtpStateId> transition)
        //    {
        //        Transition = transition;
        //    }

        //    /// <summary>
        //    /// Returns a value indicating whether or not the transition can be accepted.
        //    /// </summary>
        //    /// <param name="context">The context to use when determining the acceptance state.</param>
        //    /// <returns>true if the transition can be accepted, false if not.</returns>
        //    public bool CanAccept(SmtpSessionContext context)
        //    {
        //        return true;
        //    }

        //    /// <summary>
        //    /// The transition function to move from the previous state to the new state.
        //    /// </summary>
        //    public Func<SmtpSessionContext, SmtpStateId> Transition { get; }
        //}

        //#endregion
    }
}
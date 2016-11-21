using System;
using System.Collections;
using System.Collections.Generic;
using SmtpServer.Protocol.Text;

namespace SmtpServer.Protocol
{
    public class SmtpStateMachine : ISmtpStateMachine
    {
        readonly StateTable _stateTable;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="options">The options to assist when configuring the state machine.</param>
        /// <param name="commandFactory">The SMTP command factory.</param>
        public SmtpStateMachine(ISmtpServerOptions options, SmtpCommandFactory commandFactory)
        {
            _stateTable = new StateTable
            {
                new State(SmtpState.Initialized)
                {
#if DEBUG
                    { "DBUG", commandFactory.TryMakeDbug },
#endif
                    { "NOOP", commandFactory.TryMakeNoop },
                    { "RSET", commandFactory.TryMakeRset },
                    { "QUIT", commandFactory.TryMakeQuit },
                    { "HELO", commandFactory.TryMakeHelo, SmtpState.WaitingForMail },
                    { "EHLO", commandFactory.TryMakeEhlo, SmtpState.WaitingForMail },
                },
                new State(SmtpState.WaitingForMail)
                {
#if DEBUG
                    { "DBUG", commandFactory.TryMakeDbug },
#endif
                    { "NOOP", commandFactory.TryMakeNoop },
                    { "RSET", commandFactory.TryMakeRset },
                    { "QUIT", commandFactory.TryMakeQuit },
                    { "HELO", commandFactory.TryMakeHelo, SmtpState.WaitingForMail },
                    { "EHLO", commandFactory.TryMakeEhlo, SmtpState.WaitingForMail },
                    { "MAIL", commandFactory.TryMakeMail, SmtpState.WithinTransaction },
                    { "STARTTLS", commandFactory.TryMakeStartTls, SmtpState.WaitingForMailSecure },
                },
                new State(SmtpState.WaitingForMailSecure)
                {
#if DEBUG
                    { "DBUG", commandFactory.TryMakeDbug },
#endif
                    { "NOOP", commandFactory.TryMakeNoop },
                    { "RSET", commandFactory.TryMakeRset },
                    { "QUIT", commandFactory.TryMakeQuit },
                    { "AUTH", commandFactory.TryMakeAuth },
                    { "HELO", commandFactory.TryMakeHelo, SmtpState.WaitingForMailSecure },
                    { "EHLO", commandFactory.TryMakeEhlo, SmtpState.WaitingForMailSecure },
                    { "MAIL", commandFactory.TryMakeMail, SmtpState.WithinTransaction }
                },
                new State(SmtpState.WithinTransaction)
                {
#if DEBUG
                    { "DBUG", commandFactory.TryMakeDbug },
#endif
                    { "NOOP", commandFactory.TryMakeNoop },
                    { "RSET", commandFactory.TryMakeRset },
                    { "QUIT", commandFactory.TryMakeQuit },
                    { "RCPT", commandFactory.TryMakeRcpt, SmtpState.CanAcceptData },
                },
                new State(SmtpState.CanAcceptData)
                {
#if DEBUG
                    { "DBUG", commandFactory.TryMakeDbug },
#endif
                    { "NOOP", commandFactory.TryMakeNoop },
                    { "RSET", commandFactory.TryMakeRset },
                    { "QUIT", commandFactory.TryMakeQuit },
                    { "RCPT", commandFactory.TryMakeRcpt },
                    { "DATA", commandFactory.TryMakeData, SmtpState.WaitingForMail },
                }
            };

            if (options.AllowUnsecureAuthentication)
            {
                _stateTable[SmtpState.WaitingForMail].Add("AUTH", commandFactory.TryMakeAuth);
            }

            _stateTable.Initialize(SmtpState.Initialized);
        }

        /// <summary>
        /// Remove the specified command from the state.
        /// </summary>
        /// <param name="state">The SMTP state to remove the command from.</param>
        /// <param name="command">The command to remove from the state.</param>
        void ISmtpStateMachine.RemoveCommand(SmtpState state, string command)
        {
            if (_stateTable[state].Actions.ContainsKey(command))
            {
                _stateTable[state].Actions.Remove(command);
            }
        }

        /// <summary>
        /// Advances the enumerator to the next command in the stream.
        /// </summary>
        /// <param name="tokenEnumerator">The token enumerator to accept the command from.</param>
        /// <param name="command">The command that was found.</param>
        /// <param name="errorResponse">The error response that indicates why a command could not be accepted.</param>
        /// <returns>true if a valid command was found, false if not.</returns>
        public bool TryAccept(TokenEnumerator tokenEnumerator, out SmtpCommand command, out SmtpResponse errorResponse)
        {
            return _stateTable.TryAccept(tokenEnumerator, out command, out errorResponse);
        }

        class StateTable : IEnumerable
        {
            readonly Dictionary<SmtpState, State> _states = new Dictionary<SmtpState, State>();
            State _state;

            /// <summary>
            /// Sets the initial state.
            /// </summary>
            /// <param name="stateId">The ID of the initial state.</param>
            public void Initialize(SmtpState stateId)
            {
                _state = _states[stateId];
            }

            /// <summary>
            /// Returns the state with the given ID.
            /// </summary>
            /// <param name="stateId">The state ID to return.</param>
            /// <returns>The state with the given id.</returns>
            public State this[SmtpState stateId]
            {
                get { return _states[stateId]; }
            }

            /// <summary>
            /// Add the given state.
            /// </summary>
            /// <param name="state"></param>
            public void Add(State state)
            {
                _states.Add(state.StateId, state);
            }

            /// <summary>
            /// Advances the enumerator to the next command in the stream.
            /// </summary>
            /// <param name="tokenEnumerator">The token enumerator to accept the command from.</param>
            /// <param name="command">The command that is defined within the token enumerator.</param>
            /// <param name="errorResponse">The error that indicates why the command could not be made.</param>
            /// <returns>true if a valid command was found, false if not.</returns>
            public bool TryAccept(TokenEnumerator tokenEnumerator, out SmtpCommand command, out SmtpResponse errorResponse)
            {
                // lookup the correct action
                Tuple<State.TryMakeDelegate, SmtpState> action;
                if (_state.Actions.TryGetValue(tokenEnumerator.Peek().Text, out action) == false)
                {
                    var response = $"expected {String.Join("/", _state.Actions.Keys)}";

                    command = null;
                    errorResponse = new SmtpResponse(SmtpReplyCode.SyntaxError, response);

                    return false;
                }

                if (action.Item1(tokenEnumerator, out command, out errorResponse) == false)
                {
                    return false;
                }
                
                // transition to the next state
                _state = _states[action.Item2];
                return true;
            }

            /// <summary>
            /// Returns an enumerator that iterates through a collection.
            /// </summary>
            /// <returns>An <see cref="T:System.Collections.IEnumerator"/> object that can be used to iterate through the collection.</returns>
            IEnumerator IEnumerable.GetEnumerator()
            {
                // this is just here for the collection initializer syntax to work
                throw new NotImplementedException();
            }
        }

        class State : IEnumerable
        {
            public delegate bool TryMakeDelegate(TokenEnumerator enumerator, out SmtpCommand command, out SmtpResponse errorResponse);

            /// <summary>
            /// Constructor.
            /// </summary>
            /// <param name="stateId">The ID of the state.</param>
            public State(SmtpState stateId)
            {
                StateId = stateId;
                Actions = new Dictionary<string, Tuple<TryMakeDelegate, SmtpState>>(StringComparer.CurrentCultureIgnoreCase);
            }

            /// <summary>
            /// Add a state action.
            /// </summary>
            /// <param name="command">The name of the SMTP command.</param>
            /// <param name="tryMake">The function callback to create the command.</param>
            /// <param name="transitionTo">The state to transition to.</param>
            public void Add(string command, TryMakeDelegate tryMake, SmtpState? transitionTo = null)
            {
                Actions.Add(command, Tuple.Create(tryMake, transitionTo ?? StateId));
            }

            /// <summary>
            /// Returns an enumerator that iterates through a collection.
            /// </summary>
            /// <returns>An <see cref="T:System.Collections.IEnumerator"/> object that can be used to iterate through the collection.</returns>
            IEnumerator IEnumerable.GetEnumerator()
            {
                // this is just here for the collection initializer syntax to work
                throw new NotImplementedException();
            }

            /// <summary>
            /// Gets ID of the state.
            /// </summary>
            public SmtpState StateId { get; }

            /// <summary>
            /// Gets the actions that are available to the state.
            /// </summary>
            public Dictionary<string, Tuple<TryMakeDelegate, SmtpState>> Actions { get; }
        }
    }
}

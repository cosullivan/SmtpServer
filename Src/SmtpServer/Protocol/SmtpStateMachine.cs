using System;
using System.Collections;
using System.Collections.Generic;
using SmtpServer.Protocol.Text;

namespace SmtpServer.Protocol
{
    public class SmtpStateMachine
    {
        const int Initialized = 0;
        const int WaitingForMail = 1;
        const int WaitingForMailSecure = 2;
        const int WithinTransaction = 3;
        const int CanAcceptData = 4;

        readonly StateTable _stateTable;
        
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="commandFactory">The SMTP command factory.</param>
        public SmtpStateMachine(SmtpCommandFactory commandFactory)
        {
            _stateTable = new StateTable
            {
                new State(Initialized)
                {
                    { "DBUG", commandFactory.TryMakeDbug },
                    { "NOOP", commandFactory.TryMakeNoop },
                    { "RSET", commandFactory.TryMakeRset },
                    { "QUIT", commandFactory.TryMakeQuit },
                    { "HELO", commandFactory.TryMakeHelo, WaitingForMail },
                    { "EHLO", commandFactory.TryMakeEhlo, WaitingForMail },
                },
                new State(WaitingForMail)
                {
                    { "DBUG", commandFactory.TryMakeDbug },
                    { "NOOP", commandFactory.TryMakeNoop },
                    { "RSET", commandFactory.TryMakeRset },
                    { "QUIT", commandFactory.TryMakeQuit },
                    { "HELO", commandFactory.TryMakeHelo, WaitingForMail },
                    { "EHLO", commandFactory.TryMakeEhlo, WaitingForMail },
                    { "MAIL", commandFactory.TryMakeMail, WithinTransaction },
                    { "STARTTLS", commandFactory.TryMakeStartTls, WaitingForMailSecure },
                },
                new State(WaitingForMailSecure)
                {
                    { "DBUG", commandFactory.TryMakeDbug },
                    { "NOOP", commandFactory.TryMakeNoop },
                    { "RSET", commandFactory.TryMakeRset },
                    { "QUIT", commandFactory.TryMakeQuit },
                    { "AUTH", commandFactory.TryMakeAuth },
                    { "HELO", commandFactory.TryMakeHelo, WaitingForMailSecure },
                    { "EHLO", commandFactory.TryMakeEhlo, WaitingForMailSecure },
                    { "MAIL", commandFactory.TryMakeMail, WithinTransaction }
                },
                new State(WithinTransaction)
                {
                    { "DBUG", commandFactory.TryMakeDbug },
                    { "NOOP", commandFactory.TryMakeNoop },
                    { "RSET", commandFactory.TryMakeRset },
                    { "QUIT", commandFactory.TryMakeQuit },
                    { "RCPT", commandFactory.TryMakeRcpt, CanAcceptData },
                },
                new State(CanAcceptData)
                {
                    { "DBUG", commandFactory.TryMakeDbug },
                    { "NOOP", commandFactory.TryMakeNoop },
                    { "RSET", commandFactory.TryMakeRset },
                    { "QUIT", commandFactory.TryMakeQuit },
                    { "RCPT", commandFactory.TryMakeRcpt },
                    { "DATA", commandFactory.TryMakeData, WaitingForMail },
                }
            };

            _stateTable.Initialize(Initialized);
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
            readonly Dictionary<int, State> _states = new Dictionary<int, State>();
            State _state;

            /// <summary>
            /// Sets the initial state.
            /// </summary>
            /// <param name="stateId">The ID of the initial state.</param>
            public void Initialize(int stateId)
            {
                _state = _states[stateId];
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
                Tuple<State.TryMakeDelegate, int> action;
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

            readonly int _stateId;
            readonly Dictionary<string, Tuple<TryMakeDelegate, int>> _actions = new Dictionary<string, Tuple<TryMakeDelegate, int>>(StringComparer.InvariantCultureIgnoreCase);

            /// <summary>
            /// Constructor.
            /// </summary>
            /// <param name="stateId">The ID of the state.</param>
            public State(int stateId)
            {
                _stateId = stateId;
            }

            /// <summary>
            /// Add a state action.
            /// </summary>
            /// <param name="command">The name of the SMTP command.</param>
            /// <param name="tryMake">The function callback to create the command.</param>
            /// <param name="transitionTo">The state to transition to.</param>
            public void Add(string command, TryMakeDelegate tryMake, int? transitionTo = null)
            {
                Actions.Add(command, Tuple.Create(tryMake, transitionTo ?? _stateId));
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
            public int StateId
            {
                get {  return _stateId; }
            }

            /// <summary>
            /// Gets the actions that are available to the state.
            /// </summary>
            public Dictionary<string, Tuple<TryMakeDelegate, int>> Actions
            {
                get { return _actions; }
            }
        }
    }
}

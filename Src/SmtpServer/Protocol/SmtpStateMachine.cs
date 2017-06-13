﻿using System;
using System.Collections;
using System.Collections.Generic;
using SmtpServer.Text;

namespace SmtpServer.Protocol
{
    internal class SmtpStateMachine 
    {
        readonly ISmtpServerOptions _options;
        readonly SmtpSessionContext _context;
        readonly StateTable _stateTable;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="options">The options to assist when configuring the state machine.</param>
        /// <param name="context">The SMTP server session context.</param>
        internal SmtpStateMachine(ISmtpServerOptions options, SmtpSessionContext context)
        {
            _options = options;
            _context = context;
            _context.SessionAuthenticated += OnSessionAuthenticated;
            _stateTable = new StateTable
            {
                new State(SmtpState.Initialized)
                {
                    { NoopCommand.Command, TryMakeNoop },
                    { RsetCommand.Command, TryMakeRset },
                    { QuitCommand.Command, TryMakeQuit },
                    { HeloCommand.Command, TryMakeHelo, SmtpState.WaitingForMail },
                    { EhloCommand.Command, TryMakeEhlo, SmtpState.WaitingForMail },
                },
                new State(SmtpState.WaitingForMail)
                {
                    { NoopCommand.Command, TryMakeNoop },
                    { RsetCommand.Command, TryMakeRset },
                    { QuitCommand.Command, TryMakeQuit },
                    { HeloCommand.Command, TryMakeHelo, SmtpState.WaitingForMail },
                    { EhloCommand.Command, TryMakeEhlo, SmtpState.WaitingForMail }
                },
                new State(SmtpState.WaitingForMailSecure)
                {
                    { NoopCommand.Command, TryMakeNoop },
                    { RsetCommand.Command, TryMakeRset },
                    { QuitCommand.Command, TryMakeQuit },
                    { AuthCommand.Command, TryMakeAuth },
                    { HeloCommand.Command, TryMakeHelo, SmtpState.WaitingForMailSecure },
                    { EhloCommand.Command, TryMakeEhlo, SmtpState.WaitingForMailSecure }
                },
                new State(SmtpState.WithinTransaction)
                {
                    { NoopCommand.Command, TryMakeNoop },
                    { RsetCommand.Command, TryMakeRset },
                    { QuitCommand.Command, TryMakeQuit },
                    { RcptCommand.Command, TryMakeRcpt, SmtpState.CanAcceptData },
                },
                new State(SmtpState.CanAcceptData)
                {
                    { NoopCommand.Command, TryMakeNoop },
                    { RsetCommand.Command, TryMakeRset },
                    { QuitCommand.Command, TryMakeQuit },
                    { RcptCommand.Command, TryMakeRcpt },
                    { DataCommand.Command, TryMakeData, SmtpState.WaitingForMail },
                }
            };

            if (options.AllowUnsecureAuthentication)
            {
                WaitingForMail.Add(AuthCommand.Command, TryMakeAuth);
            }

            if (options.AuthenticationRequired == false)
            {
                WaitingForMail.Add(MailCommand.Command, TryMakeMail, SmtpState.WithinTransaction);
                WaitingForMailSecure.Add(MailCommand.Command, TryMakeMail, SmtpState.WithinTransaction);
            }
            else
            {
                WaitingForMail.Add(MailCommand.Command, MakeResponse(SmtpResponse.AuthenticationRequired));
                WaitingForMailSecure.Add(MailCommand.Command, MakeResponse(SmtpResponse.AuthenticationRequired));
            }

            if (options.ServerCertificate != null)
            {
                WaitingForMail.Add(StartTlsCommand.Command, TryMakeStartTls, SmtpState.WaitingForMailSecure);
            }

            _stateTable.Initialize(SmtpState.Initialized);
        }

        /// <summary>
        /// Called when the session has been authenticated.
        /// </summary>
        /// <param name="sender">The object that raised the event.</param>
        /// <param name="eventArgs">The event data.</param>
        void OnSessionAuthenticated(object sender, EventArgs eventArgs)
        {
            _context.SessionAuthenticated -= OnSessionAuthenticated;

            WaitingForMail.Clear(AuthCommand.Command);
            WaitingForMailSecure.Clear(AuthCommand.Command);

            if (_options.AuthenticationRequired)
            {
                WaitingForMail.Add(MailCommand.Command, TryMakeMail, SmtpState.WithinTransaction);
                WaitingForMailSecure.Add(MailCommand.Command, TryMakeMail, SmtpState.WithinTransaction);
            }
        }

        /// <summary>
        /// Make a delegate to return a known response.
        /// </summary>
        /// <param name="errorResponse">The error response to return.</param>
        /// <returns>The delegate that will return the correct error response.</returns>
        static State.TryMakeDelegate MakeResponse(SmtpResponse errorResponse)
        {
            return (TokenEnumerator enumerator, out SmtpCommand command, out SmtpResponse response) =>
            {
                command = null;
                response = errorResponse;

                return false;
            };
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
        
        /// <summary>
        /// Try to make a HELO command.
        /// </summary>
        /// <param name="tokenEnumerator">The token enumerator to use when matching the command.</param>
        /// <param name="command">The command that was found.</param>
        /// <param name="errorResponse">The error response that was returned if a command could not be matched.</param>
        /// <returns>true if a HELO command was found, false if not.</returns>
        bool TryMakeHelo(TokenEnumerator tokenEnumerator, out SmtpCommand command, out SmtpResponse errorResponse)
        {
            return new SmtpParser(_options, tokenEnumerator).TryMakeHelo(out command, out errorResponse);
        }

        /// <summary>
        /// Try to make a EHLO command.
        /// </summary>
        /// <param name="tokenEnumerator">The token enumerator to use when matching the command.</param>
        /// <param name="command">The command that was found.</param>
        /// <param name="errorResponse">The error response that was returned if a command could not be matched.</param>
        /// <returns>true if a EHLO command was found, false if not.</returns>
        bool TryMakeEhlo(TokenEnumerator tokenEnumerator, out SmtpCommand command, out SmtpResponse errorResponse)
        {
            return new SmtpParser(_options, tokenEnumerator).TryMakeEhlo(out command, out errorResponse);
        }

        /// <summary>
        /// Try to make a NOOP command.
        /// </summary>
        /// <param name="tokenEnumerator">The token enumerator to use when matching the command.</param>
        /// <param name="command">The command that was found.</param>
        /// <param name="errorResponse">The error response that was returned if a command could not be matched.</param>
        /// <returns>true if a NOOP command was found, false if not.</returns>
        bool TryMakeNoop(TokenEnumerator tokenEnumerator, out SmtpCommand command, out SmtpResponse errorResponse)
        {
            return new SmtpParser(_options, tokenEnumerator).TryMakeNoop(out command, out errorResponse);
        }

        /// <summary>
        /// Try to make a QUIT command.
        /// </summary>
        /// <param name="tokenEnumerator">The token enumerator to use when matching the command.</param>
        /// <param name="command">The command that was found.</param>
        /// <param name="errorResponse">The error response that was returned if a command could not be matched.</param>
        /// <returns>true if a QUIT command was found, false if not.</returns>
        bool TryMakeQuit(TokenEnumerator tokenEnumerator, out SmtpCommand command, out SmtpResponse errorResponse)
        {
            return new SmtpParser(_options, tokenEnumerator).TryMakeQuit(out command, out errorResponse);
        }

        /// <summary>
        /// Try to make a RSET command.
        /// </summary>
        /// <param name="tokenEnumerator">The token enumerator to use when matching the command.</param>
        /// <param name="command">The command that was found.</param>
        /// <param name="errorResponse">The error response that was returned if a command could not be matched.</param>
        /// <returns>true if a RSET command was found, false if not.</returns>
        bool TryMakeRset(TokenEnumerator tokenEnumerator, out SmtpCommand command, out SmtpResponse errorResponse)
        {
            return new SmtpParser(_options, tokenEnumerator).TryMakeRset(out command, out errorResponse);
        }

        /// <summary>
        /// Try to make a AUTH command.
        /// </summary>
        /// <param name="tokenEnumerator">The token enumerator to use when matching the command.</param>
        /// <param name="command">The command that was found.</param>
        /// <param name="errorResponse">The error response that was returned if a command could not be matched.</param>
        /// <returns>true if a AUTH command was found, false if not.</returns>
        bool TryMakeAuth(TokenEnumerator tokenEnumerator, out SmtpCommand command, out SmtpResponse errorResponse)
        {
            return new SmtpParser(_options, tokenEnumerator).TryMakeAuth(out command, out errorResponse);
        }

        /// <summary>
        /// Try to make a STARTTLS command.
        /// </summary>
        /// <param name="tokenEnumerator">The token enumerator to use when matching the command.</param>
        /// <param name="command">The command that was found.</param>
        /// <param name="errorResponse">The error response that was returned if a command could not be matched.</param>
        /// <returns>true if a STARTTLS command was found, false if not.</returns>
        bool TryMakeStartTls(TokenEnumerator tokenEnumerator, out SmtpCommand command, out SmtpResponse errorResponse)
        {
            return new SmtpParser(_options, tokenEnumerator).TryMakeStartTls(out command, out errorResponse);
        }

        /// <summary>
        /// Try to make a MAIL command.
        /// </summary>
        /// <param name="tokenEnumerator">The token enumerator to use when matching the command.</param>
        /// <param name="command">The command that was found.</param>
        /// <param name="errorResponse">The error response that was returned if a command could not be matched.</param>
        /// <returns>true if a MAIL command was found, false if not.</returns>
        bool TryMakeMail(TokenEnumerator tokenEnumerator, out SmtpCommand command, out SmtpResponse errorResponse)
        {
            return new SmtpParser(_options, tokenEnumerator).TryMakeMail(out command, out errorResponse);
        }

        /// <summary>
        /// Try to make a RCPT command.
        /// </summary>
        /// <param name="tokenEnumerator">The token enumerator to use when matching the command.</param>
        /// <param name="command">The command that was found.</param>
        /// <param name="errorResponse">The error response that was returned if a command could not be matched.</param>
        /// <returns>true if a RCPT command was found, false if not.</returns>
        bool TryMakeRcpt(TokenEnumerator tokenEnumerator, out SmtpCommand command, out SmtpResponse errorResponse)
        {
            return new SmtpParser(_options, tokenEnumerator).TryMakeRcpt(out command, out errorResponse);
        }

        /// <summary>
        /// Try to make a DATA command.
        /// </summary>
        /// <param name="tokenEnumerator">The token enumerator to use when matching the command.</param>
        /// <param name="command">The command that was found.</param>
        /// <param name="errorResponse">The error response that was returned if a command could not be matched.</param>
        /// <returns>true if a DATA command was found, false if not.</returns>
        bool TryMakeData(TokenEnumerator tokenEnumerator, out SmtpCommand command, out SmtpResponse errorResponse)
        {
            return new SmtpParser(_options, tokenEnumerator).TryMakeData(out command, out errorResponse);
        }

        /// <summary>
        /// Returns the waiting for mail state.
        /// </summary>
        State WaitingForMail => _stateTable[SmtpState.WaitingForMail];

        /// <summary>
        /// Returns the waiting for mail in a secure transaction state.
        /// </summary>
        State WaitingForMailSecure => _stateTable[SmtpState.WaitingForMailSecure];

        #region StateTable

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
            public State this[SmtpState stateId] => _states[stateId];

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

        #endregion

        #region State

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
                Actions = new Dictionary<string, Tuple<TryMakeDelegate, SmtpState>>(StringComparer.OrdinalIgnoreCase);
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
            /// Add a state action.
            /// </summary>
            /// <param name="command">The name of the SMTP command.</param>
            /// <param name="tryMake">The function callback to create the command.</param>
            /// <param name="transitionTo">The state to transition to.</param>
            public void Replace(string command, TryMakeDelegate tryMake, SmtpState? transitionTo = null)
            {
                Clear(command);
                Add(command, tryMake, transitionTo);
            }

            /// <summary>
            /// Clear the command from the current state.
            /// </summary>
            /// <param name="command">The command to clear.</param>
            public void Clear(string command)
            {
                Actions.Remove(command);
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

        #endregion
    }
}
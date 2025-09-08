using System;
using System.Collections;
using System.Collections.Generic;
using SmtpServer.Protocol;

namespace SmtpServer.StateMachine
{
    internal sealed class SmtpStateTable : IEnumerable
    {
        internal static readonly SmtpStateTable Shared = new SmtpStateTable
        {
            new SmtpState(SmtpStateId.Initialized)
            {
                { NoopCommand.Command },
                { RsetCommand.Command },
                { QuitCommand.Command },
                { ProxyCommand.Command },
                { HeloCommand.Command, WaitingForMailSecureWhenSecure },
                { EhloCommand.Command, WaitingForMailSecureWhenSecure }
            },
            new SmtpState(SmtpStateId.WaitingForMail)
            {
                { NoopCommand.Command },
                { RsetCommand.Command },
                { QuitCommand.Command },
                { StartTlsCommand.Command, CanAcceptStartTls, SmtpStateId.WaitingForMailSecure },
                { AuthCommand.Command, context => context.EndpointDefinition.AllowUnsecureAuthentication && context.Authentication.IsAuthenticated == false },
                { HeloCommand.Command, SmtpStateId.WaitingForMail },
                { EhloCommand.Command, SmtpStateId.WaitingForMail },
                { MailCommand.Command, SmtpStateId.WithinTransaction }
            },
            new SmtpState(SmtpStateId.WaitingForMailSecure)
            {
                { NoopCommand.Command },
                { RsetCommand.Command },
                { QuitCommand.Command },
                { AuthCommand.Command, context => context.Authentication.IsAuthenticated == false },
                { HeloCommand.Command, SmtpStateId.WaitingForMailSecure },
                { EhloCommand.Command, SmtpStateId.WaitingForMailSecure },
                { MailCommand.Command, SmtpStateId.WithinTransaction }
            },
            new SmtpState(SmtpStateId.WithinTransaction)
            {
                { NoopCommand.Command },
                { RsetCommand.Command, WaitingForMailSecureWhenSecure },
                { QuitCommand.Command },
                { RcptCommand.Command, SmtpStateId.CanAcceptData },
            },
            new SmtpState(SmtpStateId.CanAcceptData)
            {
                { NoopCommand.Command },
                { RsetCommand.Command, WaitingForMailSecureWhenSecure },
                { QuitCommand.Command },
                { RcptCommand.Command },
                { DataCommand.Command, SmtpStateId.WaitingForMail },
            }
        };

        static SmtpStateId WaitingForMailSecureWhenSecure(SmtpSessionContext context)
        {
            return context.Pipe.IsSecure ? SmtpStateId.WaitingForMailSecure : SmtpStateId.WaitingForMail;
        }

        static bool CanAcceptStartTls(SmtpSessionContext context)
        {
            return context.EndpointDefinition.CertificateFactory != null && context.Pipe.IsSecure == false;
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

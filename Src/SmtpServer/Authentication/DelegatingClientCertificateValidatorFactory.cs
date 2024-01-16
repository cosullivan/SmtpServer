using System;

namespace SmtpServer.Authentication
{
    public class DelegatingClientCertificateValidatorFactory : IClientCertificateValidatorFactory
    {
        readonly Func<ISessionContext, IClientCertificateValidator> _delegate;

        public DelegatingClientCertificateValidatorFactory(Func<ISessionContext, IClientCertificateValidator> @delegate)
        {
            _delegate = @delegate;
        }

        /// <summary>
        /// Creates an instance of the service for the given session context.
        /// </summary>
        /// <param name="context">The session context.</param>
        /// <returns>The service instance for the session context.</returns>
        public IClientCertificateValidator CreateInstance(ISessionContext context)
        {
            return _delegate(context);
        }
    }
}
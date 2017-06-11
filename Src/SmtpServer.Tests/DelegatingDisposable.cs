using System;

namespace SmtpServer.Tests
{
    internal sealed class DelegatingDisposable : IDisposable
    {
        readonly Action _delegate;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="delegate">The delegate to execute upon disposal.</param>
        public DelegatingDisposable(Action @delegate)
        {
            _delegate = @delegate;
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            _delegate();
        }
    }
}
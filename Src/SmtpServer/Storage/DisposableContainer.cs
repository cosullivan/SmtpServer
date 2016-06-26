using System;

namespace SmtpServer.Storage
{
    internal sealed class DisposableContainer<TInstance> : IDisposable
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="instance">The instance to dispose.</param>
        internal DisposableContainer(TInstance instance)
        {
            Instance = instance;
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            if (Instance is IDisposable)
            {
                ((IDisposable)Instance).Dispose();
            }
        }

        /// <summary>
        /// Returns the instance.
        /// </summary>
        internal TInstance Instance { get; }
    }
}
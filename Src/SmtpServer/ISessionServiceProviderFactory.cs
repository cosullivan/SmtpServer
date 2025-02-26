using System;
using System.Threading.Tasks;

namespace SmtpServer
{
    /// <summary>
    /// Allows customization of the IServiceProvider instance that is used in the session context.
    /// </summary>
    public interface ISessionServiceProviderFactory
    {
        /// <summary>
        /// Creates an IServiceProvider instance for one session context.
        /// </summary>
        IServiceProvider CreateServiceProvider(IServiceProvider rootServiceProvider);
    }
}

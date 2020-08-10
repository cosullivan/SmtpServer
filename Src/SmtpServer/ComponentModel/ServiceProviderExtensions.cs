using SmtpServer.Protocol;
using System;

namespace SmtpServer.ComponentModel
{
    internal static class ServiceProviderExtensions
    {
        internal static TService GetService<TService>(this IServiceProvider serviceProvider, Func<TService> serviceFactory) where TService : class
        {
            if (serviceProvider == null)
            {
                throw new ArgumentNullException(nameof(serviceProvider));
            }

            return serviceProvider.GetService(typeof(ISmtpCommandFactory)) as TService ?? serviceFactory();
        }

        internal static TService GetServiceOrDefault<TService>(this IServiceProvider serviceProvider, TService @default) where TService : class
        {
            if (serviceProvider == null)
            {
                throw new ArgumentNullException(nameof(serviceProvider));
            }

            return serviceProvider.GetService(typeof(TService)) as TService ?? @default;
        }
    }
}
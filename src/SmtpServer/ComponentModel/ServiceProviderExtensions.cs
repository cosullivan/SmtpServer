using System;

namespace SmtpServer.ComponentModel
{
    internal static class ServiceProviderExtensions
    {
        internal static TService GetServiceOrDefault<TService>(this IServiceProvider serviceProvider, TService @default) where TService : class
        {
            if (serviceProvider == null)
            {
                throw new ArgumentNullException(nameof(serviceProvider));
            }

            return serviceProvider.GetService(typeof(TService)) as TService ?? @default;
        }

        internal static TService GetService<TServiceFactory, TService>(this IServiceProvider serviceProvider, ISessionContext sessionContext, TService @default) 
            where TService : class
            where TServiceFactory : ISessionContextInstanceFactory<TService>
        {
            if (serviceProvider == null)
            {
                throw new ArgumentNullException(nameof(serviceProvider));
            }

            if (serviceProvider.GetService(typeof(TServiceFactory)) is TServiceFactory sessionFactory)
            {
                return sessionFactory.CreateInstance(sessionContext);
            }

            return serviceProvider.GetServiceOrDefault(@default);
        }
    }
}

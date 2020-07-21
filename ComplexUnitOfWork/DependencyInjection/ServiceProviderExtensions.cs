using System;

using SimpleInjector;
using SimpleInjector.Lifestyles;

namespace ComplexUnitOfWork.DependencyInjection
{
    public static class ServiceProviderExtensions
    {
        public static IDisposable BeginScope(this IServiceProvider serviceProvider)
        {
            if (serviceProvider is Container container)
            {
                return AsyncScopedLifestyle.BeginScope(container);
            }

            throw new NotSupportedException("Not supported container type!");
        }

        public static TService GetService<TService>(this IServiceProvider serviceProvider)
        {
            return (TService)serviceProvider.GetService(typeof(TService));
        }

    }
}

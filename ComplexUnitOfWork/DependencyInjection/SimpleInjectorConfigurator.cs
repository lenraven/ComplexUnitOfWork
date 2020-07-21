using System;

using ComplexUnitOfWork.Repositories;
using ComplexUnitOfWork.Repositories.Cache;
using ComplexUnitOfWork.Repositories.SQL;
using ComplexUnitOfWork.Scheduler;
using ComplexUnitOfWork.UnitOfWork;
using ComplexUnitOfWork.UnitOfWork.Database;

using Hangfire;
using Hangfire.SqlServer;

using Microsoft.Extensions.Caching.Memory;

using SimpleInjector;
using SimpleInjector.Lifestyles;

namespace ComplexUnitOfWork.DependencyInjection
{
    public static class SimpleInjectorConfigurator
    {
        public static IServiceProvider CreateContainer()
        {
            var container = new Container();

            container.Options.EnableAutoVerification = false;
            container.Options.DefaultScopedLifestyle = new AsyncScopedLifestyle();

            container.RegisterSingleton<IMemoryCache>(() => new MemoryCache(new MemoryCacheOptions()));

            container.Register<SampleContext>(Lifestyle.Scoped);
            container.Register<IUnitOfWork, UnitOfWork.Implementation.UnitOfWork>(Lifestyle.Scoped);
            container.Register<IDbConnectionProvider, UnitOfWork.Implementation.UnitOfWork>(Lifestyle.Scoped);
            container.Register<ICachedUnitOfWork, UnitOfWork.Implementation.UnitOfWork>(Lifestyle.Scoped);
            container.Collection.Register<IUnitOfWorkComponent>(new [] {typeof(SampleContext), typeof(HangfireScheduler)}, Lifestyle.Scoped);

            container.Register(() => new SqlServerStorage(container.GetInstance<IDbConnectionProvider>().CurrentConnection), Lifestyle.Scoped);
            container.Register<IBackgroundJobClient>(() => new BackgroundJobClient(container.GetInstance<SqlServerStorage>()), Lifestyle.Scoped);

            container.Register<IScheduler, HangfireScheduler>(Lifestyle.Scoped);
            
            container.Register<ISampleRepository, SampleSqlRepository>();
            container.Register<ILocaleRepository, LocaleSqlRepository>();
            container.RegisterDecorator<ISampleRepository, SampleCacheRepository>();
            container.RegisterDecorator<ILocaleRepository, LocaleCacheRepository>();

            return container;
        }
    }
}

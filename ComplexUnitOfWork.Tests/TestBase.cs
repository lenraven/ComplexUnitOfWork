using System;
using System.Threading;
using System.Transactions;

using ComplexUnitOfWork.DependencyInjection;

using NUnit.Framework;

using SimpleInjector;

namespace ComplexUnitOfWork.Tests
{
    public abstract class TestBase
    {
        private readonly AsyncLocal<IServiceProvider> _serviceProviderContainer = new AsyncLocal<IServiceProvider>();

        protected IServiceProvider ServiceProvider => _serviceProviderContainer.Value;

        [SetUp]
        public void SetUp()
        {
            _serviceProviderContainer.Value = (Container)SimpleInjectorConfigurator.CreateContainer();
        }

        [TearDown]
        public void TearDown()
        {
            if (_serviceProviderContainer.Value is Container container)
                container.Dispose();
        }

        protected IDisposable CreateTransaction()
        {
            return new TransactionScope(TransactionScopeOption.Required,
                new TransactionOptions { IsolationLevel = IsolationLevel.ReadCommitted },
                TransactionScopeAsyncFlowOption.Enabled);
        }
    }
}

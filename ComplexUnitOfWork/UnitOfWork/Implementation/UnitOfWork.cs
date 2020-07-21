using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;

using ComplexUnitOfWork.UnitOfWork.Database;

using Microsoft.EntityFrameworkCore;

namespace ComplexUnitOfWork.UnitOfWork.Implementation
{
    public class UnitOfWork : IUnitOfWork, ICachedUnitOfWork, IDisposable, IDbConnectionProvider
    {
        private readonly SampleContext _dbContext;
        private readonly IEnumerable<IUnitOfWorkComponent> _components;

        public UnitOfWork(SampleContext dbContext, IEnumerable<IUnitOfWorkComponent> components)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _components = components ?? throw new ArgumentNullException(nameof(components));
        }

        public async Task CompleteAsync()
        {
            try
            {
                using (var scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions {IsolationLevel = IsolationLevel.ReadCommitted}, TransactionScopeAsyncFlowOption.Enabled))
                {
                    var changedComponents = _components.Where(c => c.HasChanges).ToArray();
                    foreach (var component in changedComponents)
                    {
                        await component.CompleteAsync();
                    }
                    scope.Complete();
                }
            }
            catch
            {
                Rollback();
                throw;
            }
        }

        public void Rollback()
        {
            foreach (var component in _components)
            {
                component.Rollback();
            }
        }

        public void Attach<TEntity>(TEntity entity)
        {
            _dbContext.Attach(entity);
        }

        public void Dispose()
        {
            _dbContext.Dispose();
        }

        public DbConnection CurrentConnection => _dbContext.Database.GetDbConnection();
    }
}

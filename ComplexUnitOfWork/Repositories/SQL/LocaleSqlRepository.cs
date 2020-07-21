using System;
using System.Threading.Tasks;

using ComplexUnitOfWork.Model;
using ComplexUnitOfWork.UnitOfWork.Database;

namespace ComplexUnitOfWork.Repositories.SQL
{
    public class LocaleSqlRepository : ILocaleRepository
    {
        private readonly SampleContext _context;

        public LocaleSqlRepository(SampleContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public void Add(Locale instance)
        {
            _context.Locales.Add(instance);
        }

        public Task<Locale> GetAsync(string id)
        {
            return _context.Locales.FindAsync(id).AsTask();
        }
    }
}

using System;
using System.Threading.Tasks;

using ComplexUnitOfWork.Model;
using ComplexUnitOfWork.UnitOfWork.Database;

namespace ComplexUnitOfWork.Repositories.SQL
{
    public class SampleSqlRepository : ISampleRepository
    {
        private readonly SampleContext _context;

        public SampleSqlRepository(SampleContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public void Add(Sample sampleInstance)
        {
            _context.Samples.Add(sampleInstance);
        }

        public Task<Sample> GetAsync(string id)
        {
            return _context.Samples.FindAsync(id).AsTask();
        }
    }
}

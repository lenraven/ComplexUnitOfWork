using System.Threading.Tasks;

using ComplexUnitOfWork.Model;

namespace ComplexUnitOfWork.Repositories
{
    public interface ISampleRepository
    {
        void Add(Sample sampleInstance);
        Task<Sample> GetAsync(string id);
    }
}
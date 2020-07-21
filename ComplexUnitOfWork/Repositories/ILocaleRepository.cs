using System.Threading.Tasks;

using ComplexUnitOfWork.Model;

namespace ComplexUnitOfWork.Repositories
{
    public interface ILocaleRepository
    {
        void Add(Locale instance);
        Task<Locale> GetAsync(string id);
    }
}
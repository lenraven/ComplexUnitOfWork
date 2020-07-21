using System.Threading.Tasks;

namespace ComplexUnitOfWork.UnitOfWork
{
    public interface IUnitOfWork
    {
        Task CompleteAsync();
        void Rollback();
    }
}

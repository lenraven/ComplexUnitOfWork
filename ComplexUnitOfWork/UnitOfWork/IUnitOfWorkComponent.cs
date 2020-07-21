using System.Threading.Tasks;

namespace ComplexUnitOfWork.UnitOfWork
{
    public interface IUnitOfWorkComponent
    {
        bool HasChanges { get; }
        Task CompleteAsync();
        void Rollback();
    }
}
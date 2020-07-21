using System.Data.Common;

namespace ComplexUnitOfWork.UnitOfWork
{
    public interface IDbConnectionProvider
    {
        DbConnection CurrentConnection { get; }
    }
}

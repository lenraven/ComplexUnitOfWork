namespace ComplexUnitOfWork.UnitOfWork
{
    public interface ICachedUnitOfWork
    {
        void Attach<TEntity>(TEntity entity);
    }
}
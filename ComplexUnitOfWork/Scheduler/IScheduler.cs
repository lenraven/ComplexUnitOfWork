using System;
using System.Linq.Expressions;

namespace ComplexUnitOfWork.Scheduler
{
    public interface IScheduler
    {
        void Schedule(string queue, Expression<Action> operation);
    }
}

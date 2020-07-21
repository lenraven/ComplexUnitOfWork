using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

using ComplexUnitOfWork.UnitOfWork;

using Hangfire;
using Hangfire.States;

namespace ComplexUnitOfWork.Scheduler
{
    public class HangfireScheduler : IScheduler, IUnitOfWorkComponent
    {
        private readonly IBackgroundJobClient _jobClient;

        private readonly ICollection<(string queue, Expression<Action> operation)> _operations = new List<(string queue, Expression<Action> operation)>();

        public HangfireScheduler(IBackgroundJobClient jobClient)
        {
            _jobClient = jobClient ?? throw new ArgumentNullException(nameof(jobClient));
        }

        public void Schedule(string queue, Expression<Action> operation)
        {
            _operations.Add((queue, operation));
        }

        public bool HasChanges => _operations.Any();

        public Task CompleteAsync()
        {
            foreach (var operation in _operations)
            {
                var state = new EnqueuedState(operation.queue);
                _jobClient.Create(operation.operation, state);
            }
            return Task.CompletedTask;
        }

        public void Rollback()
        {
            _operations.Clear();
        }
    }
}
using System;
using System.Linq;
using System.Threading.Tasks;

using ComplexUnitOfWork.DependencyInjection;
using ComplexUnitOfWork.Model;
using ComplexUnitOfWork.Repositories;
using ComplexUnitOfWork.Scheduler;
using ComplexUnitOfWork.UnitOfWork;
using ComplexUnitOfWork.UnitOfWork.Database;

using Hangfire;
using Hangfire.SqlServer;

using Microsoft.EntityFrameworkCore;

using NUnit.Framework;

namespace ComplexUnitOfWork.Tests.UnitOfWork
{
    [TestFixture]
    public class Scenarios : TestBase
    {
        [Test]
        public async Task UsingEntityFromCache()
        {
            using (CreateTransaction())
            {
                using (ServiceProvider.BeginScope())
                {
                    // Warmup cache
                    var repository = ServiceProvider.GetService<ILocaleRepository>();
                    await repository.GetAsync("en-GB");
                }

                using (ServiceProvider.BeginScope())
                {
                    var localeRepository = ServiceProvider.GetService<ILocaleRepository>();
                    var locale = await localeRepository.GetAsync("en-GB");
                    
                    var sampleInstance = new Sample
                    {
                        Id = "sample1",
                        Name = "First Sample",
                        Locale = locale
                    };

                    var repository = ServiceProvider.GetService<ISampleRepository>();

                    repository.Add(sampleInstance);
                    
                    await ServiceProvider.GetService<IUnitOfWork>().CompleteAsync();
                }
            }
        }

        [Test]
        public async Task OneRepository()
        {
            using (CreateTransaction())
            {
                using (ServiceProvider.BeginScope())
                {
                    var sampleInstance = new Sample
                    {
                        Id = "sample1",
                        Name = "First Sample"
                    };

                    var repository = ServiceProvider.GetService<ISampleRepository>();

                    repository.Add(sampleInstance);
                    
                    await ServiceProvider.GetService<IUnitOfWork>().CompleteAsync();
                }

                using (ServiceProvider.BeginScope())
                {
                    var repository = ServiceProvider.GetService<ISampleRepository>();

                    var sampleInstanceFromRepository = await repository.GetAsync("sample1");

                    Assert.That(sampleInstanceFromRepository, Is.Not.Null);
                    Assert.That(sampleInstanceFromRepository.Id, Is.EqualTo("sample1"));
                    Assert.That(sampleInstanceFromRepository.Name, Is.EqualTo("First Sample"));

                    sampleInstanceFromRepository.Name = "Modified Sample";

                    await ServiceProvider.GetService<IUnitOfWork>().CompleteAsync();
                }

                using (ServiceProvider.BeginScope())
                {
                    var repository = ServiceProvider.GetService<ISampleRepository>();

                    var sampleInstanceFromCache = await repository.GetAsync("sample1");

                    Assert.That(sampleInstanceFromCache, Is.Not.Null);
                    Assert.That(sampleInstanceFromCache.Id, Is.EqualTo("sample1"));
                    Assert.That(sampleInstanceFromCache.Name, Is.EqualTo("Modified Sample"));

                    sampleInstanceFromCache.Name = "Modified Sample Form Cache";

                    await ServiceProvider.GetService<IUnitOfWork>().CompleteAsync();
                }

                using (ServiceProvider.BeginScope())
                {
                    var repository = ServiceProvider.GetService<ISampleRepository>();

                    var sampleInstanceFromRepository = await repository.GetAsync("sample1");

                    Assert.That(sampleInstanceFromRepository, Is.Not.Null);
                    Assert.That(sampleInstanceFromRepository.Id, Is.EqualTo("sample1"));
                    Assert.That(sampleInstanceFromRepository.Name, Is.EqualTo("Modified Sample Form Cache"));
                }
            }
        }

        [Test]
        public async Task RollbackedModification()
        {
            using (CreateTransaction())
            {
                using (ServiceProvider.BeginScope())
                {
                    var sampleInstance = new Sample
                    {
                        Id = "sample2",
                        Name = "First Sample"
                    };

                    var repository = ServiceProvider.GetService<ISampleRepository>();

                    repository.Add(sampleInstance);
                    
                    await ServiceProvider.GetService<IUnitOfWork>().CompleteAsync();
                }

                using (ServiceProvider.BeginScope())
                {
                    var repository = ServiceProvider.GetService<ISampleRepository>();

                    var sampleInstanceFromRepository = await repository.GetAsync("sample2");

                    Assert.That(sampleInstanceFromRepository, Is.Not.Null);
                    Assert.That(sampleInstanceFromRepository.Id, Is.EqualTo("sample2"));
                    Assert.That(sampleInstanceFromRepository.Name, Is.EqualTo("First Sample"));

                    sampleInstanceFromRepository.Name = "Modified Sample";

                    ServiceProvider.GetService<IUnitOfWork>().Rollback();
                }

                using (ServiceProvider.BeginScope())
                {
                    var repository = ServiceProvider.GetService<ISampleRepository>();

                    var sampleInstanceFromRepository = await repository.GetAsync("sample2");

                    Assert.That(sampleInstanceFromRepository, Is.Not.Null);
                    Assert.That(sampleInstanceFromRepository.Id, Is.EqualTo("sample2"));
                    Assert.That(sampleInstanceFromRepository.Name, Is.EqualTo("First Sample"));
                }
            }
        }

        [Test]
        public async Task MultipleRepository()
        {
            using (CreateTransaction())
            {
                using (ServiceProvider.BeginScope())
                {
                    var sampleInstance = new Sample
                    {
                        Id = "sample3",
                        Name = "First Sample"
                    };

                    var sampleRepository = ServiceProvider.GetService<ISampleRepository>();
                    var localeRepository = ServiceProvider.GetService<ILocaleRepository>();

                    sampleRepository.Add(sampleInstance);
                    localeRepository.Add(new Locale {Id = "de-DE" });
                    
                    await ServiceProvider.GetService<IUnitOfWork>().CompleteAsync();
                }

                using (ServiceProvider.BeginScope())
                {
                    var sampleRepository = ServiceProvider.GetService<ISampleRepository>();
                    var localeRepository = ServiceProvider.GetService<ILocaleRepository>();

                    var sampleInstanceFromRepository = await sampleRepository.GetAsync("sample3");
                    var localeInstanceFromRepository = await localeRepository.GetAsync("de-DE");

                    Assert.That(sampleInstanceFromRepository, Is.Not.Null);
                    Assert.That(sampleInstanceFromRepository.Id, Is.EqualTo("sample3"));
                    Assert.That(sampleInstanceFromRepository.Name, Is.EqualTo("First Sample"));

                    Assert.That(localeInstanceFromRepository, Is.Not.Null);
                    Assert.That(localeInstanceFromRepository.Id, Is.EqualTo("de-DE"));

                    sampleInstanceFromRepository.Name = "Modified Sample";
                    await ServiceProvider.GetService<IUnitOfWork>().CompleteAsync();
                }

                using (ServiceProvider.BeginScope())
                {
                    var sampleRepository = ServiceProvider.GetService<ISampleRepository>();
                    var localeRepository = ServiceProvider.GetService<ILocaleRepository>();

                    var sampleInstanceFromRepository = await sampleRepository.GetAsync("sample3");
                    var localeInstanceFromRepository = await localeRepository.GetAsync("de-DE");

                    Assert.That(sampleInstanceFromRepository, Is.Not.Null);
                    Assert.That(sampleInstanceFromRepository.Id, Is.EqualTo("sample3"));
                    Assert.That(sampleInstanceFromRepository.Name, Is.EqualTo("Modified Sample"));

                    Assert.That(localeInstanceFromRepository, Is.Not.Null);
                    Assert.That(localeInstanceFromRepository.Id, Is.EqualTo("de-DE"));
                }
            }
        }

        [Test]
        public async Task HangfireAndFailedRepositorySave()
        {
            var queue = "failedtestqueue";
            var id = $"sample{Guid.NewGuid()}";

            try
            {
                using (ServiceProvider.BeginScope())
                {
                    var sampleInstance = new Sample
                    {
                        Id = id,
                        Name = "First Sample"
                    };

                    var repository = ServiceProvider.GetService<ISampleRepository>();

                    repository.Add(sampleInstance);
            
                    await ServiceProvider.GetService<IUnitOfWork>().CompleteAsync();
                }

                using (ServiceProvider.BeginScope())
                {
                    var sampleInstance = new Sample
                    {
                        Id = id,
                        Name = "First Sample"
                    };

                    var repository = ServiceProvider.GetService<ISampleRepository>();

                    repository.Add(sampleInstance);
                    ServiceProvider.GetService<IScheduler>().Schedule(queue,() => TestContext.WriteLine("TestJobExecuted!"));
            
                    Assert.ThrowsAsync<DbUpdateException>(async () => await ServiceProvider.GetService<IUnitOfWork>().CompleteAsync());
                }

                using (ServiceProvider.BeginScope())
                {
                    var monitoringApi = ServiceProvider.GetService<SqlServerStorage>().GetMonitoringApi();

                    var jobs = monitoringApi.EnqueuedJobs(queue, 0, 10);

                    if(!jobs.Any())
                        Assert.Pass("No enqueued job!");
                    else
                    {
                        Assert.That(jobs, Is.All.Property("Value").Property("State").EqualTo("Deleted"));
                    }
                }
            }
            finally
            {
                await TearDownWithHangfireAsync(queue, id);
            }
        }

        [Test]
        public async Task HangfireAndSuccessRepository()
        {
            var queue = "successtestqueue";
            var id = $"sample{Guid.NewGuid()}";
            try
            {
                using (ServiceProvider.BeginScope())
                {
                    var sampleInstance = new Sample
                    {
                        Id = id,
                        Name = "First Sample"
                    };

                    var repository = ServiceProvider.GetService<ISampleRepository>();

                    repository.Add(sampleInstance);
                    ServiceProvider.GetService<IScheduler>()
                        .Schedule(queue, () => TestContext.WriteLine("TestJobExecuted!"));

                    Assert.DoesNotThrowAsync(async () => await ServiceProvider.GetService<IUnitOfWork>().CompleteAsync());
                }

                using (ServiceProvider.BeginScope())
                {
                    var monitoringApi = ServiceProvider.GetService<SqlServerStorage>().GetMonitoringApi();

                    var jobs = monitoringApi.EnqueuedJobs(queue, 0, 10);

                    Assert.That(jobs, Has.Exactly(1).Property("Value").Property("State").Not.EqualTo("Deleted"));
                }
            }
            finally
            {
                await TearDownWithHangfireAsync(queue, id);
            }
        }

        private async Task TearDownWithHangfireAsync(string queue, string sampleId)
        {
            using (ServiceProvider.BeginScope())
            {
                var dbContext = ServiceProvider.GetService<SampleContext>();
                var sample = await dbContext.FindAsync<Sample>(sampleId);
                dbContext.Remove(sample);
                await dbContext.SaveChangesAsync();
                    
                var monitoringApi = ServiceProvider.GetService<SqlServerStorage>().GetMonitoringApi();
                var jobs = monitoringApi.EnqueuedJobs(queue, 0, 10);
                foreach (var job in jobs)
                {
                    ServiceProvider.GetService<IBackgroundJobClient>().Delete(job.Key);
                }
            }
        }
    }
}

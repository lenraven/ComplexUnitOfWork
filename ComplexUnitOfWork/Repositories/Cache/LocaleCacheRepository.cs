using System;
using System.Threading.Tasks;

using ComplexUnitOfWork.Model;
using ComplexUnitOfWork.UnitOfWork;
using ComplexUnitOfWork.UnitOfWork.Implementation;

using Microsoft.Extensions.Caching.Memory;

namespace ComplexUnitOfWork.Repositories.Cache
{
    public class LocaleCacheRepository : ILocaleRepository
    {
        private readonly ILocaleRepository _repository;
        private readonly IMemoryCache _cache;
        private readonly ICachedUnitOfWork _unitOfWork;

        public LocaleCacheRepository(ILocaleRepository repository, IMemoryCache cache, ICachedUnitOfWork unitOfWork)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        }

        public void Add(Locale instance)
        {
            _repository.Add(instance);
        }

        public async Task<Locale> GetAsync(string id)
        {
            var cacheKey = CreateCacheKey(id);
            
            var sample = await _cache.GetOrCreateAsync(cacheKey, entry =>
            {
                entry.SlidingExpiration = TimeSpan.FromHours(1);
                return _repository.GetAsync(id);
            });

            _unitOfWork.Attach(sample);
            
            return sample;
        }

        private string CreateCacheKey(string id)
        {
            return $"Locale:{id}";
        }
    }
}

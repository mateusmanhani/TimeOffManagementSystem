using MAG.TOF.Application.DTOs;
using MAG.TOF.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace MAG.TOF.Infrastructure.Services
{
    public class ExternalDataCache : IExternalDataCache
    {
        private readonly ICoreApiService _coreApiClient;
        private readonly ICacheService _cacheService;

        private const string UsersCacheKey = "users_all";
        private const string DepartmentsCacheKey = "departments_all";
        private const string GradesCacheKey = "grades_all";

        public ExternalDataCache(
            ICoreApiService coreApiClient,
            ICacheService cacheService)
        {
            _coreApiClient = coreApiClient;
            _cacheService = cacheService;
        }

        public async Task<List<UserDto>> GetCachedUsersAsync()
        {
            return await _cacheService.GetOrCreateAsync(
                key: UsersCacheKey,
                factory: async () => await _coreApiClient.GetUsersAsync(),
                expiration: TimeSpan.FromMinutes(30));
        }

        public async Task<List<DepartmentDto>> GetCachedDepartmentsAsync()
        {
            return await _cacheService.GetOrCreateAsync(
                key: DepartmentsCacheKey,
                factory: async () => await _coreApiClient.GetDepartmentsAsync(),
                expiration: TimeSpan.FromMinutes(30));
        }

        public async Task<List<GradeDto>> GetCachedGradesAsync()
        {
            return await _cacheService.GetOrCreateAsync(
                key: GradesCacheKey,
                factory: async () => await _coreApiClient.GetGradesAsync(),
                expiration: TimeSpan.FromMinutes(30));
        }
    }
}

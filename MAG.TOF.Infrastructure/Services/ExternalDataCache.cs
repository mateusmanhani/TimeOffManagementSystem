using MAG.TOF.Application.DTOs;
using MAG.TOF.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace MAG.TOF.Infrastructure.Services
{
    public class ExternalDataCache : IExternalDataCache
    {
        private readonly ICoreApiService _coreApiClient;
        private readonly ICacheService _cacheService;
        private readonly ILogger<ExternalDataCache> _logger;

        private const string UsersCacheKey = "users_all";
        private const string DepartmentsCacheKey = "departments_all";
        private const string GradesCacheKey = "grades_all";

        public ExternalDataCache(
            ICoreApiService coreApiClient,
            ICacheService cacheService,
            ILogger<ExternalDataCache> logger)
        {
            _coreApiClient = coreApiClient;
            _cacheService = cacheService;
            _logger = logger;
        }

        public async Task<List<UserDto>> GetCachedUsersAsync()
        {
            var users =  await _cacheService.GetOrCreateAsync(
                key: UsersCacheKey,
                factory: async () => await _coreApiClient.GetUsersAsync(),
                expiration: TimeSpan.FromMinutes(30));

            return users.OrderBy(u => u.FullName).ToList();
        }

        public async Task<List<DepartmentDto>> GetCachedDepartmentsAsync()
        {
            var departments = await _cacheService.GetOrCreateAsync(
                key: DepartmentsCacheKey,
                factory: async () => await _coreApiClient.GetDepartmentsAsync(),
                expiration: TimeSpan.FromMinutes(30));
            return departments.OrderBy(d => d.Name).ToList();
        }

        public async Task<List<GradeDto>> GetCachedGradesAsync()
        {
            return await _cacheService.GetOrCreateAsync(
                key: GradesCacheKey,
                factory: async () => await _coreApiClient.GetGradesAsync(),
                expiration: TimeSpan.FromMinutes(30));
        }

        public async Task<List<UserDto>> GetManagersAsync()
        {
            try
            {
                var users = await GetCachedUsersAsync();
                var grades = await GetCachedGradesAsync();

                // find the manager grade
                var managerGrade = grades.
                    FirstOrDefault(g => g.Name.Contains("Manager", StringComparison.OrdinalIgnoreCase));

                if (managerGrade == null)
                {
                    _logger.LogWarning("Manager grade not found in cached grades");
                    return new List<UserDto>();
                }

                // filter users by manager grade
                var managers = users
                    .Where(u => u.GradeId == managerGrade.Id)
                    .ToList();

                if (managers.Count == 0)
                {
                    _logger.LogWarning("No managers found in cached users");
                }

                return managers;
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Failed to load managers from cache");
                return new List<UserDto>();
            }
        }
    }
}

using ErrorOr;
using MAG.TOF.Application.DTOs;
using MAG.TOF.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace MAG.TOF.Application.Services
{
    public class ReferenceDataService
    {
        private readonly ICoreApiClient _coreApiClient;
        private readonly ICacheService _cacheService;
        private readonly ILogger<ReferenceDataService> _logger;

        private const string UsersKey = "users_all";
        private const string DepartmentsKey = "departments_all";
        private const string GradesKey = "grades_all";

        public ReferenceDataService(
            ICoreApiClient coreApiClient,
            ICacheService cacheService,
            ILogger<ReferenceDataService> logger)
        {
            _coreApiClient = coreApiClient;
            _cacheService = cacheService;
            _logger = logger;
        }

        public async Task<ErrorOr<UserDto>> ValidateUserExistsAsync(int userId)
        {
            var users = await GetCachedUsersAsync();
            var user = users.FirstOrDefault(u => u.Id == userId);

            if (user == null)
            {
                _logger.LogWarning("User with Id {UserId} does not exist", userId);
                return Error.NotFound("Request.UserNotFound", $"User with ID {userId} does not exist");
            }

            return user;
        }

        public async Task<ErrorOr<DepartmentDto>> ValidateDepartmentExistsAsync(int departmentId)
        {
            var departments = await GetCachedDepartmentsAsync();
            var department = departments.FirstOrDefault(d => d.Id == departmentId);

            if (department == null)
            {
                _logger.LogWarning("Department with Id {DepartmentId} does not exist", departmentId);
                return Error.NotFound("Request.DepartmentNotFound", $"Department with ID {departmentId} does not exist");
            }

            return department;
        }

        public async Task<ErrorOr<UserDto>> ValidateManagerExistsAndHasCorrectGradeAsync(int managerId)
        {
            // Step 1: Check if user exists
            var userResult = await ValidateUserExistsAsync(managerId);
            if (userResult.IsError)
            {
                return userResult.Errors;
            }

            var manager = userResult.Value;

            // Step 2: Get grades
            var grades = await GetCachedGradesAsync();

            // Step 3: Check if user has manager grade
            var isManagerGrade = grades.Any(g =>
                g.Id == manager.GradeId &&
                g.Name.Contains("Manager", StringComparison.OrdinalIgnoreCase));

            if (!isManagerGrade)
            {
                _logger.LogWarning("User {ManagerId} (GradeId: {GradeId}) is not a manager",
                    managerId, manager.GradeId);
                return Error.Validation("Request.InvalidManager",
                    $"User '{manager.FullName}' does not have manager privileges.");
            }

            _logger.LogDebug("Manager validation passed: {ManagerName} (ID: {ManagerId})",
                manager.FullName, manager.Id);

            return manager;
        }

        // Helper methods for caching
        public async Task<List<UserDto>> GetCachedUsersAsync()
        {
            return await _cacheService.GetOrCreateAsync(
                key: UsersKey,
                factory: async () => await _coreApiClient.GetUsersAsync(),
                expiration: TimeSpan.FromMinutes(30));
        }

        public async Task<List<DepartmentDto>> GetCachedDepartmentsAsync()
        {
            return await _cacheService.GetOrCreateAsync(
                key: DepartmentsKey,
                factory: async () => await _coreApiClient.GetDepartmentsAsync(),
                expiration: TimeSpan.FromMinutes(30));
        }

        public async Task<List<GradeDto>> GetCachedGradesAsync()
        {
            return await _cacheService.GetOrCreateAsync(
                key: GradesKey,
                factory: async () => await _coreApiClient.GetGradesAsync(),
                expiration: TimeSpan.FromMinutes(30));
        }
    }
}

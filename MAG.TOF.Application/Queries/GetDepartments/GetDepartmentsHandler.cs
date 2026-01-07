using ErrorOr;
using MAG.TOF.Application.DTOs;
using MAG.TOF.Application.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace MAG.TOF.Application.Queries.GetDepartments
{
    public class GetDepartmentsHandler : IRequestHandler<GetDepartmentsQuery, ErrorOr<List<DepartmentDto>>>
    {

        private readonly ICoreApiClient _coreApiClient;
        private readonly ICacheService _cacheService;
        private readonly ILogger<GetDepartmentsHandler> _logger;

        // Cache key for departments
        private const string CacheKey = "departments_all";

        public GetDepartmentsHandler(
            ICoreApiClient coreApiClient, 
            ICacheService cacheService, 
            ILogger<GetDepartmentsHandler> logger)
        {
            _coreApiClient = coreApiClient;
            _cacheService = cacheService;
            _logger = logger;
        }

        public async Task<ErrorOr<List<DepartmentDto>>> Handle(GetDepartmentsQuery query, 
            CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Fetching departments from CORE API");
                var departments = await _cacheService.GetOrCreateAsync(
                    key : CacheKey,
                    factory : async () => await _coreApiClient.GetDepartmentsAsync(),
                    expiration : TimeSpan.FromMinutes(30)
                    );
                _logger.LogInformation("Successfully fetched {Departments} departments from CORE API", departments.Count);
                return departments;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP request error while fetching departments from CORE API");
                return Error.Failure("Departments.FetchFailed", "Failed to retrieve departments from core API");
            }
            catch (Exception ex) 
            {
                _logger.LogError(ex, "Unexpected error while fetching departments");
                return Error.Failure("Departments.UnexpectedError", "An unexpected error occurred while fetching departments");
            }
        }
    }
}

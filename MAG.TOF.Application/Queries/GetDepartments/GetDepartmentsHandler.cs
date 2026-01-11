using ErrorOr;
using MAG.TOF.Application.DTOs;
using MAG.TOF.Application.Interfaces;
using MAG.TOF.Application.Services;
using MediatR;
using Microsoft.Extensions.Logging;

namespace MAG.TOF.Application.Queries.GetDepartments
{
    public class GetDepartmentsHandler : IRequestHandler<GetDepartmentsQuery, ErrorOr<List<DepartmentDto>>>
    {

        private readonly IExternalDataCache _externalDataCache;
        private readonly ILogger<GetDepartmentsHandler> _logger;

        public GetDepartmentsHandler(
            IExternalDataCache externalDataCache,
            ILogger<GetDepartmentsHandler> logger)
        {
            _externalDataCache = externalDataCache;
            _logger = logger;
        }

        public async Task<ErrorOr<List<DepartmentDto>>> Handle(GetDepartmentsQuery query, 
            CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Fetching departments from CORE API");
                var departments = await _externalDataCache.GetCachedDepartmentsAsync();

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

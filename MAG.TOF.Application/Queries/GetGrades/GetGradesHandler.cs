using ErrorOr;
using MAG.TOF.Application.DTOs;
using MAG.TOF.Application.Interfaces;
using MAG.TOF.Application.Services;
using MediatR;
using Microsoft.Extensions.Logging;

namespace MAG.TOF.Application.Queries.GetGrades
{
    public class GetGradesHandler : IRequestHandler<GetGradesQuery, ErrorOr<List<GradeDto>>>
    {

        private readonly IExternalDataCache _externalDataCache;
        private readonly ILogger<GetGradesHandler> _logger;

        public GetGradesHandler(
            IExternalDataCache externalDataCache,
            ILogger<GetGradesHandler> logger)
        {
            _externalDataCache = externalDataCache;
            _logger = logger;
        }
        public async Task<ErrorOr<List<GradeDto>>> Handle(GetGradesQuery request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Fetching grades from CORE API");
                var grades = await _externalDataCache.GetCachedGradesAsync();

                _logger.LogInformation("Successfully fetched {Count} grades from CORE API", grades.Count);
                return grades;

            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP request error while fetching grades from CORE API");
                return Error.Failure("Grades.FetchFailed", "Failed to retrieve grades from core API");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while fetching grades");
                return Error.Failure("Grades.UnexpectedError", "An unexpected error occurred while fetching grades");
            }
        }
    }
}

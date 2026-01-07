using ErrorOr;
using MAG.TOF.Application.DTOs;
using MAG.TOF.Application.Interfaces;
using MAG.TOF.Application.Queries.GetDepartments;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MAG.TOF.Application.Queries.GetGrades
{
    public class GetGradesHandler : IRequestHandler<GetGradesQuery, ErrorOr<List<GradeDto>>>
    {

        private readonly ICoreApiClient _coreApiClient;
        private readonly ICacheService _cacheService;
        private readonly ILogger<GetGradesHandler> _logger;

        private const string CacheKey = "gardes_all";

        public GetGradesHandler(
            ICoreApiClient coreApiClient,
            ICacheService cacheService,
            ILogger<GetGradesHandler> logger)
        {
            _coreApiClient = coreApiClient;
            _cacheService = cacheService;
            _logger = logger;
        }
        public async Task<ErrorOr<List<GradeDto>>> Handle(GetGradesQuery request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Fetching grades from CORE API");
                var grades = await _cacheService.GetOrCreateAsync(
                    key : CacheKey,
                    factory : async () => await _coreApiClient.GetGradesAsync(),
                    expiration : TimeSpan.FromMinutes(30)
                    );

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

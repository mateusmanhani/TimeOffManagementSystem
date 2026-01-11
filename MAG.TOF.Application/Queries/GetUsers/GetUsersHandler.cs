using ErrorOr;
using MAG.TOF.Application.DTOs;
using MAG.TOF.Application.Interfaces;
using MAG.TOF.Application.Services;
using MediatR;
using Microsoft.Extensions.Logging;

namespace MAG.TOF.Application.Queries.GetUsers
{
    public class GetUsersHandler : IRequestHandler<GetUsersQuery, ErrorOr<List<UserDto>>>
    {
        private readonly ILogger<GetUsersHandler> _logger;
        private readonly IExternalDataCache _externalDataCache;

        public GetUsersHandler(
            IExternalDataCache externalDataCache,
            ILogger<GetUsersHandler> logger)
        {
            _externalDataCache = externalDataCache;
            _logger = logger;
        }

        public async Task<ErrorOr<List<UserDto>>> Handle(GetUsersQuery query, CancellationToken cancelationToken)
        {
            try
            {
                _logger.LogInformation("Fetching users (with caching)");

                var users = await _externalDataCache.GetCachedUsersAsync();

                _logger.LogInformation("Successfully fetched {UserCount} users", users.Count);
                return users;

            }
            catch(HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP request error while fetching users");
                return Error.Failure("Users.FetchFailed", "Failed to retrieve users from CORE API");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while fetching users");
                return Error.Failure("Users.UnexpectedError", "An unexpected error occurred while fetching users");
            }
        }
    }
}

using ErrorOr;
using MAG.TOF.Application.DTOs;
using MAG.TOF.Application.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace MAG.TOF.Application.Queries.GetUsers
{
    public class GetUsersHandler : IRequestHandler<GetUsersQuery, ErrorOr<List<UserDto>>>
    {
        private readonly ICoreApiClient _coreApiClient;
        private readonly ICacheService _cacheService;
        private readonly ILogger<GetUsersHandler> _logger;

        // Cache key for users
        private const string CacheKey = "users_all";

        public GetUsersHandler(
            ICoreApiClient coreApiClient,
            ICacheService cacheService,
            ILogger<GetUsersHandler> logger)
        {
            _coreApiClient = coreApiClient;
            _cacheService = cacheService;
            _logger = logger;
        }

        public async Task<ErrorOr<List<UserDto>>> Handle(GetUsersQuery query, CancellationToken cancelationToken)
        {
            try
            {
                _logger.LogInformation("Fetching users (with caching)");

                var users = await _cacheService.GetOrCreateAsync(
                    key: CacheKey,
                    factory: async () => await _coreApiClient.GetUsersAsync(),
                    expiration: TimeSpan.FromMinutes(30)
                 );

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

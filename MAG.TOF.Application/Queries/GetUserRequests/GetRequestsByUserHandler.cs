using ErrorOr;
using MAG.TOF.Application.Interfaces;
using MAG.TOF.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;

namespace MAG.TOF.Application.Queries.GetUserRequests
{
    public class GetRequestsByUserHandler : IRequestHandler<GetRequestsByUserQuery, ErrorOr<List<Request>>>
    {
        private readonly IRequestRepository _repository;
        private readonly ILogger<GetRequestsByUserHandler> _logger;

        public GetRequestsByUserHandler(IRequestRepository repository, ILogger<GetRequestsByUserHandler> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<ErrorOr<List<Request>>> Handle(GetRequestsByUserQuery query, CancellationToken cancellationToken)
        {
            try
            {
                // Validate UserId
                if (query.UserId <= 0)
                {
                    _logger.LogWarning("Invalid UserId {UserId}", query.UserId);
                    return Error.Validation("InvalidUserId", "The provided UserId is invalid.");
                }

                // Call Repository to get requests by UserId
                var requests = await _repository.GetRequestsByUserIdAsync(query.UserId);
                
                // Handle empty results
                if (requests == null)
                {
                    _logger.LogInformation("No requests found for user with ID {UserId}", query.UserId);
                }
                return requests;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving requests for user with ID {UserId}", query.UserId);
                return Error.Validation("RequestRetrievalError", "An error occurred while retrieving the requests.");
            }
        }
    }
}

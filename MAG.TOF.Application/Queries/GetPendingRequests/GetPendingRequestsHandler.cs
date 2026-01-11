using ErrorOr;
using MAG.TOF.Application.Interfaces;
using MAG.TOF.Application.Services;
using MAG.TOF.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;

namespace MAG.TOF.Application.Queries.GetPendingRequests
{
    public class GetPendingRequestsHandler : IRequestHandler<GetPendingRequestsQuery, ErrorOr<List<Request>>>
    {
        private readonly IRequestRepository _repository;
        private readonly ExternalDataValidator _externalDataValidator;
        private readonly ILogger<GetPendingRequestsHandler> _logger;

        public GetPendingRequestsHandler(
            IRequestRepository requestRepository,
            ExternalDataValidator externalDataValidator,
            ILogger<GetPendingRequestsHandler> logger)
        {
            _repository = requestRepository;
            _externalDataValidator = externalDataValidator;
            _logger = logger;
        }

        public async Task<ErrorOr<List<Request>>> Handle(GetPendingRequestsQuery query, CancellationToken cancellationToken)
        {
            try
            {
                // Validate input
                if (query.LoggedUserId <= 0)
                {
                    _logger.LogWarning("Invalid LoggedUserID: {LoggedUserId}", query.LoggedUserId) ;
                    return Error.Validation("Manager.InvalidId", "Manager ID must be positive");
                }

                // Ensure LoggedUserId has Manager grade
                var managerResult = await _externalDataValidator.ValidateManagerExistsAndHasCorrectGradeAsync(query.LoggedUserId);
                if (managerResult.IsError)
                {
                    _logger.LogWarning("User {UserId} is not a valid manager or does not exist.", query.LoggedUserId);
                    return managerResult.Errors;
                }

                // Fetch pending requests for the manager
                var requests = await _repository.GetPendingRequestsByManagerId(query.LoggedUserId);

                _logger.LogInformation("Found {Count} pending requests for manager {ManagerId}", 
                    requests.Count, query.LoggedUserId);

                return requests;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching pending requests for manager: {ManagerId}", query.LoggedUserId);
                return Error.Failure("PendingRequests.FetchFailed", "An error occurred while fetching pending requests");
            }
        }
    }

}

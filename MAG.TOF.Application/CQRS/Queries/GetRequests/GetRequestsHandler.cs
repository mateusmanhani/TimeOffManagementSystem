using ErrorOr;
using MAG.TOF.Application.Interfaces;
using MAG.TOF.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;

namespace MAG.TOF.Application.CQRS.Queries.GetRequests
{
    public class GetRequestsHandler : IRequestHandler<GetRequestQuery, ErrorOr<List<Request>>>
    {
        private readonly IRequestRepository _repository;
        private readonly ILogger<GetRequestsHandler> _logger;

        public GetRequestsHandler(
            IRequestRepository repository,
            ILogger<GetRequestsHandler> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<ErrorOr<List<Request>>> Handle(GetRequestQuery query, CancellationToken cancellationToken)
        {
            try
            {
                var requests = await _repository.GetRequestsAsync(
                    managerId: query.ManagerId,
                    status: query.Status,
                    userId: query.UserId,
                    from: query.From,
                    to: query.To,
                    page: query.Page,
                    pageSize: query.PageSize,
                    cancellationToken: cancellationToken);

                _logger.LogInformation("Retrieved {Count} requests.", requests.Count);
                return requests;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving requests.");
                return Error.Failure("An error occurred while retrieving requests.");
            }
        }
    }
}

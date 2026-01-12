using ErrorOr;
using MAG.TOF.Domain.Entities;
using MediatR;

namespace MAG.TOF.Application.CQRS.Queries.GetPendingRequests
{
    public record GetPendingRequestsQuery(
        int LoggedUserId
        ) : IRequest<ErrorOr<List<Request>>>;
}

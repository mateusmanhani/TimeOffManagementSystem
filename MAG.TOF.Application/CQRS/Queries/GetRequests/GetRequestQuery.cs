using ErrorOr;
using MAG.TOF.Domain.Entities;
using MAG.TOF.Domain.Enums;
using MediatR;

namespace MAG.TOF.Application.CQRS.Queries.GetRequests
{
    public record GetRequestQuery(
        int? ManagerId = null,
        RequestStatus? Status = null,
        int? UserId = null,
        DateTime? From = null,
        DateTime? To = null,
        int? Page = null,
        int? PageSize = null) : IRequest<ErrorOr<List<Request>>>;
}

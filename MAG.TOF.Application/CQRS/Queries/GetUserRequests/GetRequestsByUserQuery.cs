using ErrorOr;
using MAG.TOF.Domain.Entities;
using MediatR;

namespace MAG.TOF.Application.CQRS.Queries.GetUserRequests
{
    public record GetRequestsByUserQuery(
        int UserId
        ) : IRequest<ErrorOr<List<Request>>>;
    
}

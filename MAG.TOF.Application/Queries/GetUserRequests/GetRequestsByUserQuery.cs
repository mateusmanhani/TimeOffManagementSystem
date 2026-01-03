using ErrorOr;
using MAG.TOF.Domain.Entities;
using MediatR;

namespace MAG.TOF.Application.Queries.GetUserRequests
{
    public record GetRequestsByUserQuery(
        int UserId
        ) : IRequest<ErrorOr<List<Request>>>;
    
}

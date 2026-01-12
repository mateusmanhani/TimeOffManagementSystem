using ErrorOr;
using MAG.TOF.Domain.Enums;
using MediatR;

namespace MAG.TOF.Application.CQRS.Commands.UpdateRequest
{
    public record UpdateRequestCommand(
        int LoggedUserId,
        int RequestId,
        DateTime StartDate,
        DateTime EndDate,
        int? ManagerId,
        RequestStatus Status 
        ) : IRequest<ErrorOr<Success>>;
}

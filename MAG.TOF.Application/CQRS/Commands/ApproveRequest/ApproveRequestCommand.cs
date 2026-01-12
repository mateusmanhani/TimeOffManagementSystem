using ErrorOr;
using MediatR;

namespace MAG.TOF.Application.CQRS.Commands.ApproveRequest
{
    public record ApproveRequestCommand(
        int LoggedUserId,
        int RequestId) : IRequest<ErrorOr<Success>>;
}

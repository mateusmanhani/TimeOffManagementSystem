using ErrorOr;
using MediatR;

namespace MAG.TOF.Application.CQRS.Commands.RecallRequest
{
    public record RecallRequestCommand(
        int LoggedUserId,
        int RequestId
        ) : IRequest<ErrorOr<Success>>;
}

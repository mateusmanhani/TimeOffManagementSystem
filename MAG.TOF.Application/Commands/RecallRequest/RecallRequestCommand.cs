using ErrorOr;
using MediatR;

namespace MAG.TOF.Application.Commands.RecallRequest
{
    public record RecallRequestCommand(
        int LoggedUserId,
        int RequestId
        ) : IRequest<ErrorOr<Success>>;
}

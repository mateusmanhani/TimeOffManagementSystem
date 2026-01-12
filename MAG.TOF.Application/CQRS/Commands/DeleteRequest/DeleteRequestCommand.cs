using ErrorOr;
using MediatR;

namespace MAG.TOF.Application.CQRS.Commands.DeleteRequest
{
    public record DeleteRequestCommand(
        int LoggedUserId,
        int RequestId
        ) :IRequest<ErrorOr<Success>>;
}

using ErrorOr;
using MediatR;

namespace MAG.TOF.Application.Commands.DeleteRequest
{
    public record DeleteRequestCommand(
        int LoggedUserId,
        int RequestId
        ) :IRequest<ErrorOr<Success>>;
}

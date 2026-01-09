using ErrorOr;
using MediatR;

namespace MAG.TOF.Application.Commands.RejectRequest
{
    public record RejectRequestCommand(
        int LoggedUserId,
        int RequestId,
        string RejectionReason) : IRequest<ErrorOr<Success>>;
}

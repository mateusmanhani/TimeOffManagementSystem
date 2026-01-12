using ErrorOr;
using MediatR;

namespace MAG.TOF.Application.CQRS.Commands.RejectRequest
{
    public record RejectRequestCommand(
        int LoggedUserId,
        int RequestId,
        string RejectionReason) : IRequest<ErrorOr<Success>>;
}

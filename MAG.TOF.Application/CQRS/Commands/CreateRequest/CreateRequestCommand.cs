using ErrorOr;
using MAG.TOF.Domain.Enums;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MAG.TOF.Application.CQRS.Commands.CreateRequest
{
    // Command to create a new Request; Return id of created Request
    public record CreateRequestCommand(
        int UserId,
        int DepartmentId,
        DateTime StartDate,
        DateTime EndDate,
        int? ManagerId,
        RequestStatus Status = RequestStatus.Pending  // Changed from int StatusId, default to Pending
        ) : IRequest<ErrorOr<int>>;

}

using ErrorOr;
using MAG.TOF.Domain.Enums;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MAG.TOF.Application.Commands.UpdateRequest
{
    public record UpdateRequestCommand(
        int RequestId,
        DateTime StartDate,
        DateTime EndDate,
        int? ManagerId,
        RequestStatus Status  // Changed from int StatusId
        ) : IRequest<ErrorOr<Success>>;
}

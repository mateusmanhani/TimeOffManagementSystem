using ErrorOr;
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
        int StatusId
        ) : IRequest<ErrorOr<Success>>;
}

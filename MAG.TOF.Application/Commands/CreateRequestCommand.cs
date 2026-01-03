using ErrorOr;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MAG.TOF.Application.Commands
{
    // Command to create a new Request; Return id of created Request
    public record CreateRequestCommand(
        int UserId,
        int DepartmentId,
        DateTime StartDate,
        DateTime EndDate,
        int ManagerId,
        int StatusId // default 1(draft) or 2(pending)? 
        ) : IRequest<ErrorOr<int>>;

}

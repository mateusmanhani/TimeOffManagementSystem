using ErrorOr;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MAG.TOF.Application.Commands.RecallRequest
{
    public record RecallRequestCommand(
        int RequestId
        ) : IRequest<ErrorOr<Success>>;
}

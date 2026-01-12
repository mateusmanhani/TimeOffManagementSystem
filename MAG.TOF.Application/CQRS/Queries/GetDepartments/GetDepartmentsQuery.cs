using ErrorOr;
using MAG.TOF.Application.DTOs;
using MediatR;

namespace MAG.TOF.Application.CQRS.Queries.GetDepartments
{
    public record GetDepartmentsQuery : IRequest<ErrorOr<List<DepartmentDto>>>;
}

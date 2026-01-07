using ErrorOr;
using MAG.TOF.Application.DTOs;
using MediatR;

namespace MAG.TOF.Application.Queries.GetDepartments
{
    public record GetDepartmentsQuery : IRequest<ErrorOr<List<DepartmentDto>>>;
}

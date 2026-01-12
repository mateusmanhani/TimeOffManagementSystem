using ErrorOr;
using MAG.TOF.Application.DTOs;
using MediatR;

namespace MAG.TOF.Application.CQRS.Queries.GetGrades
{
    public record GetGradesQuery : IRequest<ErrorOr<List<GradeDto>>>;
}

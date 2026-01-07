using ErrorOr;
using MAG.TOF.Application.DTOs;
using MediatR;

namespace MAG.TOF.Application.Queries.GetGrades
{
    public record GetGradesQuery : IRequest<ErrorOr<List<GradeDto>>>;
}

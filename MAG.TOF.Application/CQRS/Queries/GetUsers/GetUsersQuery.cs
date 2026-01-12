using ErrorOr;
using MAG.TOF.Application.DTOs;
using MediatR;

namespace MAG.TOF.Application.CQRS.Queries.GetUsers
{
    // No parameters needed - fetches all users
    public record GetUsersQuery : IRequest<ErrorOr<List<UserDto>>>;
}

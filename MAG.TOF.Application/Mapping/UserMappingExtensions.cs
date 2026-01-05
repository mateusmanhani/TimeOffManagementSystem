using MAG.TOF.Application.DTOs;
using MAG.TOF.Application.Models;

namespace MAG.TOF.Application.Mapping
{
    public static class UserMappingExtensions
    {
        public static UserDto ToDto(this CoreApiUser user)
        {
            return new UserDto
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                FullName = user.FullName,
                GradeId = user.GradeId,
                DepartmentId = user.DepartmentId
            };
        }

        public static List<UserDto> ToDtoList(this List<CoreApiUser> users)
        {
            return users.Select(u => u.ToDto()).ToList();
        }
    }
}

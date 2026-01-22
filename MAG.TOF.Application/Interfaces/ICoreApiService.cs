using MAG.TOF.Application.DTOs;

namespace MAG.TOF.Application.Interfaces
{
    public interface ICoreApiService
    {
        public Task<List<UserDto>> GetUsersAsync();
        public Task<List<DepartmentDto>> GetDepartmentsAsync();
        public Task<List<GradeDto>> GetGradesAsync();
    }
}

using MAG.TOF.Application.DTOs;

namespace MAG.TOF.Application.Interfaces
{
    public interface IExternalDataCache
    {
        Task<List<DepartmentDto>> GetCachedDepartmentsAsync();
        Task<List<GradeDto>> GetCachedGradesAsync();
        Task<List<UserDto>> GetCachedUsersAsync();
    }
}
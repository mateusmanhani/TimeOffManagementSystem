using MAG.TOF.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MAG.TOF.Application.Interfaces
{
    public interface ICoreApiClient
    {

        public Task<List<UserDto>> GetUsersAsync();
        public Task<List<DepartmentDto>> GetDepartmentsAsync();
        public Task<List<GradeDto>> GetGradesAsync();
    }
}

using MAG.TOF.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MAG.TOF.Application.Models
{
    public class CoreApiUser
    {
        public int Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public int GradeId { get; set; }
        public int DepartmentId { get; set; }
        public int StatusId { get; set; }

        // Navigation properties
        public GradeDto Grade { get; set; } = null!;
        public DepartmentDto Department { get; set; } = null!;
    }
}

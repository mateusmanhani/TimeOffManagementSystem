using MAG.TOF.Application.DTOs;
using System.Text.Json.Serialization;

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
        //[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        //public GradeDto Grade { get; set; } = null!;
        //public DepartmentDto? Department { get; set; } = null!;
    }
}

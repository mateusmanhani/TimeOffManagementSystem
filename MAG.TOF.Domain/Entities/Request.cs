using MAG.TOF.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MAG.TOF.Domain.Entities
{
    public class Request
    {
        public int Id { get; set; }

        public int UserId { get; set; }

        public int DepartmentId { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public int TotalBusinessDays { get; set; }

        public int? ManagerId { get; set; }

        public string? ManagerComment { get; set; }

        public RequestStatus Status { get; set; }

    }
}

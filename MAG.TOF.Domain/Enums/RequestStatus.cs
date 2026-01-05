using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MAG.TOF.Domain.Enums
{
    public enum RequestStatus
    {
        Draft = 1,
        Pending = 2,
        Approved = 3,  // Fixed typo: was "Approvde"
        Rejected = 4,
        Recalled = 5
    }
}

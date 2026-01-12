using System.ComponentModel;

namespace MAG.TOF.Domain.Enums
{
    public enum RequestStatus
    {
        [Description("Draft - Not yet submitted")]
        Draft = 1,
        
        [Description("Pending - Awaiting manager approval")]
        Pending = 2,
        
        [Description("Approved - Request has been approved")]
        Approved = 3,
        
        [Description("Rejected - Request was rejected")]
        Rejected = 4,
        
        [Description("Recalled - Request was recalled by user")]
        Recalled = 5
    }
}

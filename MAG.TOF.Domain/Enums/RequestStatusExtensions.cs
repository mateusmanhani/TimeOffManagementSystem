using System.ComponentModel;
using System.Reflection;

namespace MAG.TOF.Domain.Enums
{
    public static class RequestStatusExtensions
    {
        /// <summary>
        /// Checks if the current status can transition to the target status
        /// </summary>
        public static bool CanTransitionTo(this RequestStatus current, RequestStatus target)
        {
            return (current, target) switch
            {
                // From Draft
                (RequestStatus.Draft, RequestStatus.Pending) => true,
                (RequestStatus.Draft, RequestStatus.Recalled) => true,
                
                // From Pending
                (RequestStatus.Pending, RequestStatus.Approved) => true,
                (RequestStatus.Pending, RequestStatus.Rejected) => true,
                (RequestStatus.Pending, RequestStatus.Recalled) => true,
                
                // From Approved
                (RequestStatus.Approved, RequestStatus.Recalled) => true,
                
                // No transitions allowed from Rejected or Recalled
                (RequestStatus.Rejected, _) => false,
                (RequestStatus.Recalled, _) => false,
                
                // Same status (no change)
                var (x, y) when x == y => true,
                
                // All other transitions not allowed
                _ => false
            };
        }

        /// <summary>
        /// Checks if the current status allows the request to be recalled
        /// </summary>
        public static bool CanBeRecalled(this RequestStatus status)
        {
            return status is RequestStatus.Pending;
        }

        /// <summary>
        /// Checks if the current status allows the request to be approved or rejected
        /// Only pending requests can be approved or rejected
        /// </summary>
        public static bool CanBeApprovedOrRejected(this RequestStatus status)
        {
            return status is RequestStatus.Pending or RequestStatus.Recalled;
        }

        /// <summary>
        /// Checks if the current status allows the request to be edited
        /// </summary>
        public static bool CanBeEdited(this RequestStatus status)
        {
            return status is RequestStatus.Draft or RequestStatus.Pending;
        }

        /// <summary>
        /// Checks if the current status allows the request to be deleted
        /// </summary>
        public static bool CanBeDeleted(this RequestStatus status)
        {
            return status is RequestStatus.Draft or RequestStatus.Rejected or RequestStatus.Recalled;
        }

        /// <summary>
        /// Gets a human-readable description of the status
        /// </summary>
        public static string GetDescription(this RequestStatus status)
        {
            return status.GetType()
                .GetField(status.ToString())?
                .GetCustomAttribute<DescriptionAttribute>()?
                .Description ?? status.ToString();
        }
    }
}

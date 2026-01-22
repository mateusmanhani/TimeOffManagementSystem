namespace MAG.TOF.Application.Messaging
{
    public record EmailNotificationMessage(
        string RecepientEmail,
        string Subject,
        string BodyHtml,
        DateTime StartDate,
        DateTime EndDate,
        string? ManagerAssigned = null,
        string? RequestorName = null
        );
}

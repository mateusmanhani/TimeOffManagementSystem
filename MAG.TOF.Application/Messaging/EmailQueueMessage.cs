namespace MAG.TOF.Application.Messaging
{
    public record EmailQueueMessage(
        string RequestorEmail,
        string Subject,
        string BodyHtml
        );
}

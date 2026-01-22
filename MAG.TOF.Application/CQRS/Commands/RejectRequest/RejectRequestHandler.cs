using ErrorOr;
using MAG.TOF.Application.CQRS.Commands.ApproveRequest;
using MAG.TOF.Application.Interfaces;
using MAG.TOF.Application.Messaging;
using MAG.TOF.Application.Validation;
using MAG.TOF.Domain.Enums;
using MediatR;
using Microsoft.Extensions.Logging;

namespace MAG.TOF.Application.CQRS.Commands.RejectRequest
{
    public class RejectRequestHandler : IRequestHandler<RejectRequestCommand, ErrorOr<Success>>
    {

        private readonly ExternalDataValidator _externalDataValidator;
        private readonly IRequestRepository _repository;
        private readonly IExternalDataCache _externalDataCache;
        private readonly IMessagePublisher _messagePublisher;
        private readonly ILogger<ApproveRequestHandler> _logger;

        public RejectRequestHandler(
            ExternalDataValidator externalDataValidator,
            IRequestRepository repository,
            IExternalDataCache externalDataCache,
            IMessagePublisher emailQueueService,
            ILogger<ApproveRequestHandler> logger)
        {
            _externalDataValidator = externalDataValidator;
            _repository = repository;
            _externalDataCache = externalDataCache;
            _messagePublisher = emailQueueService;
            _logger = logger;
        }
        public async Task<ErrorOr<Success>> Handle(RejectRequestCommand command, CancellationToken cancellationToken)
        {
            try
            {
                // Validade input
                if (command.RequestId < 0)
                {
                    _logger .LogError("Invalid RequestId: {RequestId}", command.RequestId);
                    return Error.Failure("Invalid RequestId");
                }

                if (command.LoggedUserId < 0)
                {
                    _logger.LogError("Invalid ManagerId: {ManagerId}", command.LoggedUserId);
                    return Error.Failure("Invalid ManagerId");
                }

                // Validate request exists
                var existingRequest = await _repository.GetRequestByIdAsync(command.RequestId, cancellationToken);
                if (existingRequest is null)
                {
                    _logger.LogError("Request not found: {RequestId}", command.RequestId);
                    return Error.Failure("Request not found");
                }

                // Validate manager authorization
                var managerResult = await _externalDataValidator.ValidateManagerExistsAndHasCorrectGradeAsync(command.LoggedUserId);
                if (managerResult.IsError)
                {
                    _logger.LogError("Manager validation failed for ManagerId: {ManagerId}", command.LoggedUserId);
                    return managerResult.Errors;
                }
                // check if logged in users is the same as the manager assigned to the request
                if (existingRequest.ManagerId != command.LoggedUserId)
                {
                    _logger.LogWarning("Manager {ManagerId} is not authorized to reject request {RequestId}", command.LoggedUserId, command.RequestId);
                    return Error.Forbidden("Request.Reject.Unauthorized", "You are not authorized to reject this request");
                }

                // Validate request can be rejected
                if (!existingRequest.Status.CanBeApprovedOrRejected())
                {
                    _logger.LogError("Request cannot be rejected in its current status: {Status}", existingRequest.Status);
                    return Error.Failure("Request cannot be rejected in its current status");
                }

                // Update request status to rejected
                existingRequest.Status = RequestStatus.Rejected;
                existingRequest.ManagerId = command.LoggedUserId;
                existingRequest.ManagerComment = command.RejectionReason;

                await _repository.UpdateRequestAsync(existingRequest, cancellationToken);
                _logger.LogInformation("Request {RequestId} rejected by Manager {ManagerId}", command.RequestId, command.LoggedUserId);

                // Try to publish message to service bus for email notification
                try
                {
                    var users = await _externalDataCache.GetCachedUsersAsync();
                    var requestor = users.FirstOrDefault(u => u.Id == existingRequest.UserId);
                    var requestorEmail = requestor?.Email;

                    if (!string.IsNullOrEmpty(requestorEmail))
                    {

                        var manager = users.FirstOrDefault(u => u.Id == command.LoggedUserId);
                        var managerName = manager?.FullName ?? "Manager";

                        var emailMsg = new EmailNotificationMessage(
                            RecepientEmail: requestorEmail,
                            Subject: "Rejected",
                            BodyHtml: $"<p>Hi {requestor?.FullName ?? "user"},</p>" +
                                      $"<p>Your request <strong>#{existingRequest.Id}</strong> from <strong>{existingRequest.StartDate:yyyy-MM-dd}</strong> to <strong>{existingRequest.EndDate:yyyy-MM-dd}</strong> has been <strong>rejected</strong>.</p>" +
                                      $"<p>Manager: <strong>{managerName}</strong></p>",
                            StartDate: existingRequest.StartDate,
                            EndDate: existingRequest.EndDate,
                            ManagerAssigned: managerName
                        );

                        await _messagePublisher.PublishAsync(emailMsg, cancellationToken);
                        _logger.LogInformation("Enqueued approval email for request {RequestId} to requestor {RequestorUserId}", existingRequest.Id, existingRequest.UserId);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to enqueue approval email for request {RequestId}", existingRequest.Id);
                }

                return Result.Success;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error rejecting request {RequestId} by manager {ManagerId}", command.RequestId, command.LoggedUserId);
                return Error.Failure("Request.RejectionFailed", "An error occurred while rejecting the request");
            }
        }
    }
}

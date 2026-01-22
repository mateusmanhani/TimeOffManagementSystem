using ErrorOr;
using MAG.TOF.Application.Interfaces;
using MAG.TOF.Application.Messaging;
using MAG.TOF.Application.Validation;
using MAG.TOF.Domain.Enums;
using MediatR;
using Microsoft.Extensions.Logging;

namespace MAG.TOF.Application.CQRS.Commands.ApproveRequest
{
    public class ApproveRequestHandler : IRequestHandler<ApproveRequestCommand, ErrorOr<Success>>
    {
        private readonly ExternalDataValidator _externalDataValidator;
        private readonly IExternalDataCache _externalDataCache;
        private readonly IMessagePublisher _messagePublisher;
        private readonly IRequestRepository _repository;
        private readonly ILogger<ApproveRequestHandler> _logger;

        public ApproveRequestHandler(
            ExternalDataValidator externalDataValidator,
            IExternalDataCache externalDataCache,
            IMessagePublisher emailQueueService,
            IRequestRepository repository,
            ILogger<ApproveRequestHandler> logger)
        {
            _externalDataValidator = externalDataValidator;
            _externalDataCache = externalDataCache;
            _messagePublisher = emailQueueService;
            _repository = repository;
            _logger = logger;
        }
        public async Task<ErrorOr<Success>> Handle(ApproveRequestCommand command, CancellationToken cancellationToken)
        {

            try
            {
                // validate input
                if (command.RequestId <= 0)
                {
                    _logger.LogWarning("Invalid RequestId {RequestId} provided by manager {ManagerId}", command.RequestId, command.LoggedUserId);
                    return Error.Validation("RequestId.Invalid", "RequestId must be a positive integer.");
                }

                // Manager id to currentUser or similar
                if (command.LoggedUserId <= 0)
                {
                    _logger.LogWarning("Invalid ManagerId {ManagerId} provided for approving request {RequestId}", command.LoggedUserId, command.RequestId);
                    return Error.Validation("ManagerId.Invalid", "ManagerId must be a positive integer.");
                }

                // Check if request exists
                var existingRequest = await _repository.GetRequestByIdAsync(command.RequestId, cancellationToken);
                if (existingRequest is null)
                {
                    _logger.LogWarning("Request {RequestId} not found for approval by manager {ManagerId}", command.RequestId, command.LoggedUserId);
                    return Error.NotFound("Request.NotFound", $"Request with ID {command.RequestId} not found");
                }

                // Check if manager exists and is valid
                var managerResult = await _externalDataValidator.ValidateManagerExistsAndHasCorrectGradeAsync(command.LoggedUserId);
                if (managerResult.IsError)
                {
                    _logger.LogWarning("Manager {ManagerId} validation failed for approving request {RequestId}", command.LoggedUserId, command.RequestId);
                    return managerResult.FirstError;
                }

                // check if logged in users is the same as the manager assigned to the request
                if (existingRequest.ManagerId != command.LoggedUserId)
                {
                    _logger.LogWarning("Manager {ManagerId} is not authorized to approve request {RequestId}", command.LoggedUserId, command.RequestId);
                    return Error.Forbidden("Request.Approve.Unauthorized", "You are not authorized to approve this request");
                }
                // Check if request can be approved
                if (!existingRequest.Status.CanBeApprovedOrRejected())
                {
                    _logger.LogWarning("Request {RequestId} cannot be approved in its current state by manager {ManagerId}", command.RequestId, command.LoggedUserId);
                    return Error.Validation("Request.CannotBeApproved",
                        $"Requests with status '{existingRequest.Status}' cannot be approved");
                }

                // Approve the request
                existingRequest.Status = RequestStatus.Approved;
                existingRequest.ManagerId = command.LoggedUserId;

                await _repository.UpdateRequestAsync(existingRequest, cancellationToken);
                _logger.LogInformation("Request {RequestId} approved by manager {ManagerId}", command.RequestId, command.LoggedUserId);

                // Try to publish email notification
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
                            Subject: "Approved",
                            BodyHtml: $"<p>Hi {requestor?.FullName ?? "user"},</p>" +
                                      $"<p>Your request <strong>#{existingRequest.Id}</strong> from <strong>{existingRequest.StartDate:yyyy-MM-dd}</strong> to <strong>{existingRequest.EndDate:yyyy-MM-dd}</strong> has been <strong>approved</strong>.</p>" +
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
                _logger.LogError(ex, "Error approving request {RequestId} by manager {ManagerId}", command.RequestId, command.LoggedUserId);
                return Error.Failure("Request.ApprovalFailed", "An error occurred while approving the request");
            }
        }
    }
}

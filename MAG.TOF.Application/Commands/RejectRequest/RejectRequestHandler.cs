using ErrorOr;
using MAG.TOF.Application.Commands.ApproveRequest;
using MAG.TOF.Application.Interfaces;
using MAG.TOF.Application.Services;
using MAG.TOF.Domain.Enums;
using MediatR;
using Microsoft.Extensions.Logging;

namespace MAG.TOF.Application.Commands.RejectRequest
{
    public class RejectRequestHandler : IRequestHandler<RejectRequestCommand, ErrorOr<Success>>
    {

        private readonly ExternalDataValidator _externalDataValidator;
        private readonly IRequestRepository _repository;
        private readonly ILogger<ApproveRequestHandler> _logger;

        public RejectRequestHandler(
            ExternalDataValidator externalDataValidator,
            IRequestRepository repository,
            ILogger<ApproveRequestHandler> logger)
        {
            _externalDataValidator = externalDataValidator;
            _repository = repository;
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
                var existingRequest = await _repository.GetRequestByIdAsync(command.RequestId);
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

                await _repository.UpdateRequestAsync(existingRequest);
                _logger.LogInformation("Request {RequestId} rejected by Manager {ManagerId}", command.RequestId, command.LoggedUserId);
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

using ErrorOr;
using MAG.TOF.Application.Interfaces;
using MAG.TOF.Application.Services;
using MAG.TOF.Domain.Enums;
using MAG.TOF.Domain.Services;
using MediatR;
using Microsoft.Extensions.Logging;

namespace MAG.TOF.Application.Commands.UpdateRequest
{
    public class UpdateRequestHandler : IRequestHandler<UpdateRequestCommand, ErrorOr<Success>>
    {
        private readonly IRequestRepository _repository;
        private readonly ILogger<UpdateRequestHandler> _logger;
        private readonly RequestValidationService _validationService;
        private readonly ReferenceDataService _referenceValidation;

        public UpdateRequestHandler(IRequestRepository repository, 
            ILogger<UpdateRequestHandler> logger, 
            RequestValidationService requestValidationService,
            ReferenceDataService referenceValidation)
        {
            _repository = repository;
            _logger = logger;
            _validationService = requestValidationService;
            _referenceValidation = referenceValidation;
        }
        
        public async Task<ErrorOr<Success>> Handle(UpdateRequestCommand command, CancellationToken cancellationToken)
        {
            try
            {
                // Validate command input
                var validationError = ValidateCommandInput(command);
                if (validationError != null)
                    return validationError.Value;

                // Fetch existing request
                var existingRequest = await _repository.GetRequestByIdAsync(command.RequestId);
                if (existingRequest == null)
                {
                    _logger.LogWarning("Request with ID {RequestId} not found", command.RequestId);
                    return Error.NotFound("RequestNotFound", $"Request with ID {command.RequestId} was not found.");
                }

                // Check if the request can be edited based on current status
                if (!existingRequest.Status.CanBeEdited())
                {
                    _logger.LogWarning("Request {RequestId} cannot be edited. Current status: {Status}", 
                        command.RequestId, existingRequest.Status);
                    return Error.Validation("CannotEditRequest", 
                        $"Requests with status '{existingRequest.Status}' cannot be edited.");
                }

                // check if the logged user is the owner of the request
                if (existingRequest.UserId != command.LoggedUserId)
                {
                    _logger.LogWarning("Only the owner of the request may update a request.");
                    return Error.Unauthorized("Unauthorized", "Only the owner of the request may update a request.");
                }

                // Ensure user cannot set himself as approving manager
                if (command.LoggedUserId == command.ManagerId)
                {
                    _logger.LogWarning("You cannot set yourself as approving manager on a request.");
                    return Error.Conflict("Request.Conflict", "You cannot set yourself as approving manager on a request.");
                }


                // Validate status transition if status is being changed
                if (existingRequest.Status != command.Status && !existingRequest.Status.CanTransitionTo(command.Status))
                {
                    _logger.LogWarning("Invalid status transition from {CurrentStatus} to {NewStatus}", 
                        existingRequest.Status, command.Status);
                    return Error.Validation("InvalidStatusTransition", 
                        $"Cannot transition from {existingRequest.Status} to {command.Status}");
                }

                // Validate New Date Range
                if (!_validationService.IsValidDateRange(command.StartDate, command.EndDate))
                {
                    _logger.LogWarning("Invalid date range: StartDate {StartDate}, EndDate {EndDate}", 
                        command.StartDate, command.EndDate);
                    return Error.Validation("InvalidDateRange", "The start date must be before the end date.");
                }

                // Update existing request
                existingRequest.StartDate = command.StartDate;
                existingRequest.EndDate = command.EndDate;
                existingRequest.Status = command.Status;

                // Validate Manager if provided
                if (command.ManagerId.HasValue)
                {
                    var managerResult = await _referenceValidation.ValidateManagerExistsAndHasCorrectGradeAsync(command.ManagerId.Value);
                    if (managerResult.IsError)
                    {
                        _logger.LogWarning("Manager validation failed for ManagerId {ManagerId}", command.ManagerId.Value);
                        return managerResult.FirstError;
                    }
                    existingRequest.ManagerId = command.ManagerId.Value;
                }

                await _repository.UpdateRequestAsync(existingRequest);
                _logger.LogInformation("Request with ID {RequestId} updated successfully", command.RequestId);
                return Result.Success;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating request with ID {RequestId}", command.RequestId);
                return Error.Failure("UpdateRequestFailed", "An error occurred while updating the request.");
            }
        }

        private ErrorOr<Success>? ValidateCommandInput(UpdateRequestCommand command)
        {
            if (command.RequestId <= 0)
            {
                _logger.LogWarning("Invalid RequestId: {RequestId}", command.RequestId);
                return Error.Validation("InvalidRequestId", "The request ID must be a positive integer.");
            }

            if (command.ManagerId.HasValue && command.ManagerId <= 0)
            {
                _logger.LogWarning("Invalid ManagerId: {ManagerId}", command.ManagerId);
                return Error.Validation("InvalidManagerId", "The manager ID must be a positive integer.");
            }

            // Validate enum using extension method
            if (!command.Status.IsValid())
            {
                _logger.LogWarning("Invalid Status: {Status}", command.Status);
                return Error.Validation("InvalidStatus", "The status must be a valid RequestStatus value.");
            }

            return null;
        }
    }
}

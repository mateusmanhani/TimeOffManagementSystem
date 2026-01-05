using ErrorOr;
using MAG.TOF.Application.Interfaces;
using MAG.TOF.Domain.Services;
using MediatR;
using Microsoft.Extensions.Logging;

namespace MAG.TOF.Application.Commands.UpdateRequest
{
    public class UpdateRequestHandler : IRequestHandler<UpdateRequestCommand, ErrorOr<Success>>
    {
        private readonly ITofRepository _repository;
        private readonly ILogger<UpdateRequestHandler> _logger;
        private readonly RequestValidationService _validationService;

        public UpdateRequestHandler(ITofRepository repository, ILogger<UpdateRequestHandler> logger, RequestValidationService requestValidationService)
        {
            _repository = repository;
            _logger = logger;
            _validationService = requestValidationService;
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

                // Validate New Date Range
                if (!_validationService.IsValidDateRange(command.StartDate, command.EndDate))
                {
                    _logger.LogWarning("Invalid date range: StartDate {StartDate}, EndDate {EndDate}", command.StartDate, command.EndDate);
                    return Error.Validation("InvalidDateRange", "The start date must be before the end date.");
                }

                // todo: Verify related entities exist (User, Department, Manager, Status)

                // Update existing request
                existingRequest.StartDate = command.StartDate;
                existingRequest.EndDate = command.EndDate;
                existingRequest.StatusId = command.StatusId;
                existingRequest.ManagerId = command.ManagerId.Value;

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

            if (command.StatusId <= 0 && command.StatusId <6)
            {
                _logger.LogWarning("Invalid StatusId: {StatusId}", command.StatusId);
                return Error.Validation("InvalidStatusId", "The status ID must be a positive integer.");
            }

            return null;
        }
    }
}

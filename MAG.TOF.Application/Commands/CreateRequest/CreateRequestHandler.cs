using ErrorOr;
using MAG.TOF.Application.Interfaces;
using MAG.TOF.Application.Services;
using MAG.TOF.Domain.Entities;
using MAG.TOF.Domain.Services;
using MediatR;
using Microsoft.Extensions.Logging;

namespace MAG.TOF.Application.Commands.CreateRequests
{
    public class CreateRequestHandler : IRequestHandler<CreateRequestCommand, ErrorOr<int>>
    {

        private readonly IRequestRepository _repository;
        private readonly ILogger<CreateRequestHandler> _logger;

        private readonly RequestValidationService _requestValidationService;
        private readonly ExternalDataValidator _externalDataValidator;


        public CreateRequestHandler(IRequestRepository repository, 
            ILogger<CreateRequestHandler> logger, 
            RequestValidationService validationService,
            ExternalDataValidator referenceValidation
            )
        {
            _repository = repository;
            _logger = logger;
            _requestValidationService = validationService;
            _externalDataValidator = referenceValidation;
        }

        public async Task<ErrorOr<int>> Handle(CreateRequestCommand command, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogDebug("Creating request for UserId: {UserId}", command.UserId);

                // Validate request and calculate business days
                var businessDaysResult = await ValidateRequestAsync(command);
                if (businessDaysResult.IsError)
                {
                    return businessDaysResult.Errors;
                }

                // Map command to entity
                var request = new Request
                {
                    UserId = command.UserId,
                    DepartmentId = command.DepartmentId,
                    StartDate = command.StartDate,
                    EndDate = command.EndDate,
                    TotalBusinessDays = businessDaysResult.Value,
                    ManagerId = command.ManagerId,
                    Status = command.Status 
                };

                // Save to database
                await _repository.AddRequestAsync(request);

                _logger.LogInformation("Successfully created request with Id: {RequestId} for UserId: {UserId}",
                    request.Id, request.UserId);
                return request.Id;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating request for UserId: {UserId}",
                    command.UserId);
                return Error.Failure("Request.CreateFailed", "An Error occurred while creating request");
            }
        }

        // Validate request, calculate/ validate business days and return business days or Errors
        private async Task<ErrorOr<int>> ValidateRequestAsync(CreateRequestCommand command)
        {
            // validate user exists (using cached data)
            var userResult = await _externalDataValidator.ValidateUserExistsAsync(command.UserId);
            if (userResult.IsError) return userResult.Errors;

            // Validate department exists
            var departmentResult = await _externalDataValidator.ValidateDepartmentExistsAsync(command.DepartmentId);
            if (departmentResult.IsError) return departmentResult.Errors;

            // Validate manager exists (if provided)
            if (command.ManagerId.HasValue)
            {
                var managerResult = await _externalDataValidator.ValidateManagerExistsAndHasCorrectGradeAsync(command.ManagerId.Value);
                if (managerResult.IsError) return managerResult.Errors;
            }


                // Ensure user cannot set himself as approving manager
                if (command.UserId == command.ManagerId)
                {
                    _logger.LogWarning("You cannot set yourself as approving manager on a request.");
                    return Error.Conflict("Request.Conflict", "You cannot set yourself as approving manager on a request.");
                }

                // validate request
                if (!_requestValidationService.IsValidDateRange(command.StartDate, command.EndDate))
           

            {
                _logger.LogWarning("Invalid date range: StartDate: {StartDate}, EndDate: {EndDate}",
                    command.StartDate, command.EndDate);
                return Error.Validation("Request.InvalidDateRange",
                    "Start date must be today or in the future, and end date must be after start date");
            }

            //  check for overlapping requests
            var overLappingRequest = await _repository.HasOverlappingRequestsAsync(
                command.UserId,
                command.StartDate,
                command.EndDate);

            if (overLappingRequest != null)
            {
                var overlapMessage = _requestValidationService.FormatOverlapMessage(
                    overLappingRequest.Id,
                    overLappingRequest.StartDate,
                    overLappingRequest.EndDate);

                _logger.LogWarning("Date overlap detected for UserId: {UserId}. {Message}",
                    command.UserId, overlapMessage);

                return Error.Conflict("Request.DateOverlap", overlapMessage);
            }

            // Calculate business days
            int actualBusinessDays = _requestValidationService.CalculateBusinessDays(
                command.StartDate,
                command.EndDate);

            _logger.LogDebug("Calculated {BusinessDays} business days for date range {StartDate} to {EndDate}",
                actualBusinessDays, command.StartDate, command.EndDate);

            //Validate minimum business days
            if (actualBusinessDays <= 0)
            {
                _logger.LogWarning("No business days in selected range");
                return Error.Validation("Request.NoBusinessDays", "Total business days must be greater than 0");
            }

            return actualBusinessDays;
        }
    }
}

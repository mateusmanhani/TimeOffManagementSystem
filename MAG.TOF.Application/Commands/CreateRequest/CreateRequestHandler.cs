using ErrorOr;
using MAG.TOF.Application.Interfaces;
using MAG.TOF.Domain.Entities;
using MAG.TOF.Domain.Services;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MAG.TOF.Application.Commands.CreateRequests
{
    public class CreateRequestHandler : IRequestHandler<CreateRequestCommand, ErrorOr<int>>
    {

        private readonly ITofRepository _repository;
        private readonly ILogger<CreateRequestHandler> _logger;
        private readonly RequestValidationService _validationService;

        public CreateRequestHandler(ITofRepository repository, ILogger<CreateRequestHandler> logger, RequestValidationService validationService)
        {
            _repository = repository;
            _logger = logger;
            _validationService = validationService;
        }

        public async Task<ErrorOr<int>> Handle(CreateRequestCommand command, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogDebug("Creating request for UserId: {UserId}", command.UserId);

                // validate request
                if (!_validationService.isValidDateRange(command.StartDate, command.EndDate))
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
                    var overlapMessage = _validationService.FormatOverlapMessage(
                        overLappingRequest.Id,
                        overLappingRequest.StartDate,
                        overLappingRequest.EndDate);

                    _logger.LogWarning("Date overlap detected for UserId: {UserId}. {Message}",
                        command.UserId, overlapMessage);

                    return Error.Conflict("Request.DateOverlap", overlapMessage);
                }

                // todo: Verify related entities exist (User, Department, Manager, Status)

                // Calculate business days
                int actualBusinessDays = _validationService.CalculateBusinessDays(
                    command.StartDate,
                    command.EndDate);

                _logger.LogDebug("Calculates {BusinessDays} business days for date range {StartDate_ to {EndDate}",
                    actualBusinessDays, command.StartDate, command.EndDate);

                //Validate minimum business days
                if (actualBusinessDays <= 0)
                {
                    _logger.LogWarning("No businesss days in selected range");
                    return Error.Validation("Request.NoBusinessDays", "Total business days must be greater than 0");
                }

                // Map command to entity
                var request = new Request
                {
                    UserId = command.UserId,
                    DepartmentId = command.DepartmentId,
                    StartDate = command.StartDate,
                    EndDate = command.EndDate,
                    TotalBusinessDays = actualBusinessDays,
                    ManagerId = command.ManagerId,
                    StatusId = command.StatusId
                };

                // Save to database
                await _repository.AddRequestAsync(request);

                _logger.LogInformation("Sucessfully created request with Id: {RequestId} for UserId: {UserId}",
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
    }
}

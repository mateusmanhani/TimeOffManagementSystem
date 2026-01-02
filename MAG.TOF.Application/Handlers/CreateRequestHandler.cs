using ErrorOr;
using MAG.TOF.Application.Commands;
using MAG.TOF.Application.Interfaces;
using MAG.TOF.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MAG.TOF.Application.Handlers
{
    public class CreateRequestHandler : IRequestHandler<CreateRequestCommand, ErrorOr<int>>
    {

        private readonly ITofRepository _repository;
        private readonly ILogger _logger;

        public CreateRequestHandler(ITofRepository repository, ILogger<CreateRequestHandler> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<ErrorOr<int>> Handle(CreateRequestCommand command, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogDebug("Creating request for UserId: {UserId}", command.UserId);

                // validate request
                if (command.EndDate < command.StartDate)
                {
                    _logger.LogWarning("EndDate {EndDate} is earlier than StartDate {StartDate} for UserId: {UserId}"
                        , command.EndDate, command.StartDate, command.UserId);
                    return Error.Validation("Request.InvalidDateRange","Start date must be before end date");
                }

                if (command.TotalBusinessDays <= 0)
                {
                    _logger.LogWarning("Invalid business days: {TotalBusinessDays}", command.TotalBusinessDays);
                    return Error.Validation("Request.InvalidBusinessDays", "Total business days must be greater than 0");
                }

                // Map command to entity
                var request = new Request
                {
                    UserId = command.UserId,
                    DepartmentId = command.DepartmentId,
                    StartDate = command.StartDate,
                    EndDate = command.EndDate,
                    TotalBusinessDays = command.TotalBusinessDays,
                    ManagerId = command.ManagerId,
                    ManagerComment = command.ManagerComment,
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

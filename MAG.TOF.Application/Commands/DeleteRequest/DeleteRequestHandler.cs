using ErrorOr;
using MAG.TOF.Application.Interfaces;
using MAG.TOF.Domain.Enums;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MAG.TOF.Application.Commands.DeleteRequest
{
    public class DeleteRequestHandler : IRequestHandler<DeleteRequestCommand, ErrorOr<Success>>
    {
        private readonly IRequestRepository _repository;
        private readonly ILogger<DeleteRequestHandler> _logger;

        public DeleteRequestHandler(IRequestRepository repository, ILogger<DeleteRequestHandler> logger)
        {
            _repository = repository;
            _logger = logger;
        }
        
        public async Task<ErrorOr<Success>> Handle(DeleteRequestCommand command, CancellationToken cancellationToken)
        {
            try
            {
                // Validate RequestId
                if (command.RequestId <= 0)
                {
                    _logger.LogWarning("Invalid Request Id: {RequestId}", command.RequestId);
                    return Error.Validation("InvalidRequestId", "Request Id must be greater than 0");
                }

                // Check if request exists
                var existingRequest = await _repository.GetRequestByIdAsync(command.RequestId);
                if (existingRequest == null)
                {
                    _logger.LogWarning("Request with id: {RequestId} not found", command.RequestId);
                    return Error.NotFound("RequestNotFound", $"Request with ID {command.RequestId} was not found.");
                }

                // Check if request can be deleted based on status
                if (!existingRequest.Status.CanBeDeleted())
                {
                    _logger.LogWarning("Request {RequestId} cannot be deleted. Current status: {Status}", 
                        command.RequestId, existingRequest.Status);
                    return Error.Validation("CannotDeleteRequest", 
                        $"Requests with status '{existingRequest.Status}' cannot be deleted. Only Draft, Rejected, or Recalled requests can be deleted.");
                }

                // Delete request
                await _repository.DeleteRequestAsync(command.RequestId);

                _logger.LogInformation("Request with ID {RequestId} deleted successfully", command.RequestId);
                return Result.Success;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting request with ID {RequestId}", command.RequestId);
                return Error.Failure("DeleteRequestFailed", "An error occurred while deleting the request.");
            }
        }
    }
}

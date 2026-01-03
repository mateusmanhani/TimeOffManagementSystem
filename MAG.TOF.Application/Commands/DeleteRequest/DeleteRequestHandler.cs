using ErrorOr;
using MAG.TOF.Application.Interfaces;
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
        private readonly ITofRepository _repository;
        private readonly ILogger<DeleteRequestHandler> _logger;

        public DeleteRequestHandler(ITofRepository repository, ILogger<DeleteRequestHandler> logger)
        {
            _repository = repository;
            _logger = logger;
        }
        public async Task<ErrorOr<Success>> Handle(DeleteRequestCommand command, CancellationToken cancellationToken)
        {
            try
            {
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
                    Error.NotFound("RequestNotFound", $"Request with ID {command.RequestId} was not found.");
                }

                // Delete request
                await _repository.DeleteRequestAsync(command.RequestId);

                _logger.LogInformation("Request with ID {RequestId} deleted successfully", command.RequestId);
                return Result.Success;


            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting request with ID {RequestId}", command.RequestId);
                return Error.Validation("DeleteRequestError", "An error occurred while deleting the request.");
            }
        }
    }
}

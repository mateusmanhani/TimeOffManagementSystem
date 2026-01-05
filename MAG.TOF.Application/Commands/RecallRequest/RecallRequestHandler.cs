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

namespace MAG.TOF.Application.Commands.RecallRequest
{
    public class RecallRequestHandler : IRequestHandler<RecallRequestCommand, ErrorOr<Success>>
    {
        private readonly IRequestRepository _repository;
        private readonly ILogger<RecallRequestHandler> _logger;
        
        public RecallRequestHandler(IRequestRepository repository, ILogger<RecallRequestHandler> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<ErrorOr<Success>> Handle(RecallRequestCommand command, CancellationToken cancellationToken)
        {
            try
            {
                // Validate RequestId
                if (command.RequestId <= 0)
                {
                    _logger.LogWarning("RecallRequestHandler: Invalid RequestId: {RequestId}", command.RequestId);
                    return Error.Validation("InvalidRequestId", "The request ID must be a positive integer.");
                }

                // Fetch existing request
                var existingRequest = await _repository.GetRequestByIdAsync(command.RequestId);
                if (existingRequest is null)
                {
                    _logger.LogWarning("RecallRequestHandler: Request with Id {RequestId} not found.", command.RequestId);
                    return Error.NotFound("RequestNotFound", $"Request with Id {command.RequestId} not found.");
                }

                // Use extension method for clean validation
                if (!existingRequest.Status.CanBeRecalled())
                {
                    _logger.LogWarning("RecallRequestHandler: Request {RequestId} cannot be recalled. Current status: {Status}", 
                        command.RequestId, existingRequest.Status);
                    return Error.Validation("InvalidStatusForRecall", 
                        $"Only pending or approved requests can be recalled. Current status: {existingRequest.Status}");
                }

                // Update request status to 'Recalled'
                existingRequest.Status = RequestStatus.Recalled;
                await _repository.UpdateRequestAsync(existingRequest);
                
                _logger.LogInformation("RecallRequestHandler: Successfully recalled request with Id {RequestId}.", command.RequestId);

                return Result.Success;
            }
            catch (Exception ex)
            { 
                _logger.LogError(ex, "RecallRequestHandler: Error recalling request with Id {RequestId}.", command.RequestId);
                return Error.Failure("RecallRequestFailed", "An error occurred while recalling the request.");
            }
        }
    }
}

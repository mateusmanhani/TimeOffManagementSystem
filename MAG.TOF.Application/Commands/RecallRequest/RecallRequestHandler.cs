using ErrorOr;
using MAG.TOF.Application.Interfaces;
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
        private readonly ITofRepository _repository;
        private readonly ILogger<RecallRequestHandler> _logger;
        public RecallRequestHandler(ITofRepository repository, ILogger<RecallRequestHandler> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<ErrorOr<Success>> Handle(RecallRequestCommand command, CancellationToken cancellationToken)
        {
            try
            {
                // Try fetching and validating request
                var existingRequest = await _repository.GetRequestByIdAsync(command.RequestId);
                if (existingRequest is null)
                {
                    _logger.LogWarning("RecallRequestHandler: Request with Id {RequestId} not found.", command.RequestId);
                    return Error.NotFound(description: $"Request with Id {command.RequestId} not found.");
                }

                // Update request status to 'Recalled' which is 5)
                existingRequest.StatusId = 5;
                await _repository.UpdateRequestAsync(existingRequest);
                
                _logger.LogInformation("RecallRequestHandler: Successfully recalled request with Id {RequestId}.", command.RequestId);

                return Result.Success;
            }
            catch (Exception ex)
            { 
                _logger.LogError(ex, "RecallRequestHandler: Error recalling request with Id {RequestId}.", command.RequestId);
                return Error.Failure(description: "An error occurred while recalling the request.");
            }

        }
    }
}

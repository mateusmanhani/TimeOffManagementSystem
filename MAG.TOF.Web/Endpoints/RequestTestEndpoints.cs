using ErrorOr;
using MAG.TOF.Application.Commands.ApproveRequest;
using MAG.TOF.Application.Commands.CreateRequests;
using MAG.TOF.Application.Commands.DeleteRequest;
using MAG.TOF.Application.Commands.RecallRequest;
using MAG.TOF.Application.Commands.RejectRequest;
using MAG.TOF.Application.Commands.UpdateRequest;
using MAG.TOF.Application.Queries.GetPendingRequests;
using MAG.TOF.Application.Queries.GetUserRequests;
using MediatR;

namespace MAG.TOF.Web.Endpoints
{
    /// <summary>
    /// Test endpoints for Request CRUD operations.
    /// </summary>
    public static class RequestTestEndpoints
    {
        public static IEndpointRouteBuilder MapRequestTestEndpoints(this IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("/api/test/requests")
                .WithTags("Request Tests")
                .DisableAntiforgery();

            // Get Pending requests for manager
            group.MapGet("/pending/manager/{managerId}", async (int loggedUserId, IMediator mediator) =>
            {
                var query = new GetPendingRequestsQuery(loggedUserId);
                var result = await mediator.Send(query);

                return result.Match(
                    requests => Results.Ok(new
                    {
                        Success = true,
                        Count = requests.Count,
                        Requests = requests
                    }),
                    errors => Results.BadRequest(new
                    {
                        Success = false,
                        Errors = errors.Select(e => new
                        {
                            Code = e.Code,
                            Description = e.Description
                        }).ToList()
                    })
                );
            })
                .WithName("GetPendingRequestsByManager");

            // Create Request
            group.MapPost("/", async (CreateRequestCommand command, IMediator mediator) =>
            {
                var result = await mediator.Send(command);

                return result.Match(
                    id => Results.Ok(new { Success = true, RequestId = id, Message = "Request created successfully" }),
                    errors => Results.BadRequest(new { Success = false, Errors = errors })
                );
            })
            .WithName("CreateTestRequest");

            // Get Requests by User
            group.MapGet("/user/{userId}", async (int userId, IMediator mediator) =>
            {
                var query = new GetRequestsByUserQuery(userId);
                var result = await mediator.Send(query);

                return result.Match(
                    requests => Results.Ok(new 
                    { 
                        Success = true, 
                        Count = requests.Count, 
                        Requests = requests 
                    }),
                    errors => Results.BadRequest(new { Success = false, Errors = errors })
                );
            })
            .WithName("GetRequestsByUser");

            // Update Request
            group.MapPut("/", async (UpdateRequestCommand command, IMediator mediator) =>
            {
                var result = await mediator.Send(command);

                return result.Match(
                    success => Results.Ok(new { Success = true, Message = "Request updated successfully" }),
                    errors => Results.BadRequest(new { Success = false, Errors = errors })
                );
            })
            .WithName("UpdateTestRequest");

            // Delete Request
            group.MapDelete("/{requestId}", async (int loggedUserId, int requestId, IMediator mediator) =>
            {
                var command = new DeleteRequestCommand(requestId, loggedUserId);
                var result = await mediator.Send(command);

                return result.Match(
                    success => Results.Ok(new { Success = true, Message = "Request deleted successfully" }),
                    errors => Results.BadRequest(new { Success = false, Errors = errors })
                );
            })
            .WithName("DeleteTestRequest");

            // Recall Request
            group.MapPatch("/{requestId}/recall", async (int loggedUserId, int requestId, IMediator mediator) =>
            {
                var command = new RecallRequestCommand(loggedUserId, requestId);
                var result = await mediator.Send(command);

                return result.Match(
                    success => Results.Ok(new { Success = true, Message = "Request recalled successfully. Status changed to Recalled." }),
                    errors => Results.BadRequest(new { Success = false, Errors = errors })
                );
            })
            .WithName("RecallTestRequest");

            // Approve Request
            group.MapPost("/{requestId}/approve", async (int requestId, ApproveRequestDto dto, IMediator mediator) =>
            {
                var command = new ApproveRequestCommand(dto.LoggedUserId, requestId);
                var result = await mediator.Send(command);

                return result.Match(
                    success => Results.Ok(new
                    {
                        Success = true,
                        Message = $"Request {requestId} approved successfully by manager {dto.LoggedUserId}"
                    }),
                    errors => Results.BadRequest(new
                    {
                        Success = false,
                        Errors = errors.Select(e => new
                        {
                            Code = e.Code,
                            Description = e.Description
                        }).ToList()
                    })
                );
            })
            .WithName("ApproveTestRequest");

            // Reject Request
            group.MapPost("/{requestId}/reject", async (int requestId, RejectRequestDto dto, IMediator mediator) =>
            {
                var command = new RejectRequestCommand(dto.LoggedUserId, requestId, dto.Reason);
                var result = await mediator.Send(command);

                return result.Match(
                    success => Results.Ok(new
                    {
                        Success = true,
                        Message = $"Request {requestId} rejected by manager {dto.LoggedUserId}",
                        RejectionReason = dto.Reason
                    }),
                    errors => Results.BadRequest(new
                    {
                        Success = false,
                        Errors = errors.Select(e => new
                        {
                            Code = e.Code,
                            Description = e.Description
                        }).ToList()
                    })
                );
            })
            .WithName("RejectTestRequest");

            return app;
        }

        // DTOS for request body binding
        public record ApproveRequestDto(int LoggedUserId);
        public record RejectRequestDto(int LoggedUserId, string Reason);
    }
}

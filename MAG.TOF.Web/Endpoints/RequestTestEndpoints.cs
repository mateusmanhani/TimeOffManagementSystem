using MAG.TOF.Application.Commands.CreateRequests;
using MAG.TOF.Application.Commands.DeleteRequest;
using MAG.TOF.Application.Commands.RecallRequest;
using MAG.TOF.Application.Commands.UpdateRequest;
using MAG.TOF.Application.Queries.GetUserRequests;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;

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
            group.MapDelete("/{requestId}", async (int requestId, IMediator mediator) =>
            {
                var command = new DeleteRequestCommand(requestId);
                var result = await mediator.Send(command);

                return result.Match(
                    success => Results.Ok(new { Success = true, Message = "Request deleted successfully" }),
                    errors => Results.BadRequest(new { Success = false, Errors = errors })
                );
            })
            .WithName("DeleteTestRequest");

            // Recall Request
            group.MapPatch("/{requestId}/recall", async (int requestId, IMediator mediator) =>
            {
                var command = new RecallRequestCommand(requestId);
                var result = await mediator.Send(command);

                return result.Match(
                    success => Results.Ok(new { Success = true, Message = "Request recalled successfully. Status changed to Recalled." }),
                    errors => Results.BadRequest(new { Success = false, Errors = errors })
                );
            })
            .WithName("RecallTestRequest");

            return app;
        }
    }
}

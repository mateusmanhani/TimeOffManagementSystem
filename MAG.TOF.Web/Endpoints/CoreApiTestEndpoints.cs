using MAG.TOF.Application.CQRS.Queries.GetDepartments;
using MAG.TOF.Application.CQRS.Queries.GetGrades;
using MAG.TOF.Application.CQRS.Queries.GetUsers;
using MediatR;

namespace MAG.TOF.Web.Endpoints
{
    /// <summary>
    /// Test endpoints for CORE API integration and caching.
    /// </summary>
    public static class CoreApiTestEndpoints
    {
        public static IEndpointRouteBuilder MapCoreApiTestEndpoints(this IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("/api/test/core")
                .WithTags("Core API Tests")
                .DisableAntiforgery();

            // Test Users - Basic
            group.MapGet("/users", async (IMediator mediator) =>
            {
                var result = await mediator.Send(new GetUsersQuery());
                
                if (!result.IsError)
                {
                    return Results.Ok(result.Value);
                }
                
                return Results.BadRequest(new 
                { 
                    Success = false, 
                    Errors = result.Errors.Select(e => new 
                    { 
                        Code = e.Code, 
                        Description = e.Description 
                    }).ToList()
                });
            })
            .WithName("GetCoreUsers"); 

            // Test Users - With Cache Info
            group.MapGet("/users/cache-test", async (IMediator mediator, ILogger<Program> logger) =>
            {
                logger.LogInformation("=== FIRST CALL - Should fetch from API ===");
                var stopwatch1 = System.Diagnostics.Stopwatch.StartNew();
                var result1 = await mediator.Send(new GetUsersQuery());
                stopwatch1.Stop();

                logger.LogInformation("=== SECOND CALL - Should come from cache ===");
                var stopwatch2 = System.Diagnostics.Stopwatch.StartNew();
                var result2 = await mediator.Send(new GetUsersQuery());
                stopwatch2.Stop();

                if (result1.IsError)
                {
                    return Results.BadRequest(new 
                    { 
                        Success = false, 
                        Errors = result1.Errors.Select(e => new 
                        { 
                            Code = e.Code, 
                            Description = e.Description 
                        }).ToList()
                    });
                }

                return Results.Ok(new
                {
                    Success = true,
                    Message = "Cache test completed. Check logs to verify cache hit on second call.",
                    FirstCallMs = stopwatch1.ElapsedMilliseconds,
                    SecondCallMs = stopwatch2.ElapsedMilliseconds,
                    CacheWorking = stopwatch2.ElapsedMilliseconds < stopwatch1.ElapsedMilliseconds,
                    UserCount = result1.Value.Count,
                    Note = "Second call should be MUCH faster (< 10ms) if cache is working"
                });
            })
            .WithName("TestUsersCaching");

            // Test Departments - Basic
            group.MapGet("/departments", async (IMediator mediator) =>
            {
                var result = await mediator.Send(new GetDepartmentsQuery());
                
                if (!result.IsError)
                {
                    return Results.Ok(result.Value);
                }
                
                return Results.BadRequest(new 
                { 
                    Success = false, 
                    Errors = result.Errors.Select(e => new 
                    { 
                        Code = e.Code, 
                        Description = e.Description 
                    }).ToList()
                });
            })
            .WithName("GetCoreDepartments");

            // Test Departments - With Cache Info
            group.MapGet("/departments/cache-test", async (IMediator mediator, ILogger<Program> logger) =>
            {
                logger.LogInformation("=== FIRST CALL - Should fetch from API ===");
                var stopwatch1 = System.Diagnostics.Stopwatch.StartNew();
                var result1 = await mediator.Send(new GetDepartmentsQuery());
                stopwatch1.Stop();

                logger.LogInformation("=== SECOND CALL - Should come from cache ===");
                var stopwatch2 = System.Diagnostics.Stopwatch.StartNew();
                var result2 = await mediator.Send(new GetDepartmentsQuery());
                stopwatch2.Stop();

                if (result1.IsError)
                {
                    return Results.BadRequest(new 
                    { 
                        Success = false, 
                        Errors = result1.Errors.Select(e => new 
                        { 
                            Code = e.Code, 
                            Description = e.Description 
                        }).ToList()
                    });
                }

                return Results.Ok(new
                {
                    Success = true,
                    FirstCallMs = stopwatch1.ElapsedMilliseconds,
                    SecondCallMs = stopwatch2.ElapsedMilliseconds,
                    CacheWorking = stopwatch2.ElapsedMilliseconds < stopwatch1.ElapsedMilliseconds,
                    DepartmentCount = result1.Value.Count
                });
            })
            .WithName("TestDepartmentsCaching");

            // Test Grades - Basic
            group.MapGet("/grades", async (IMediator mediator) =>
            {
                var result = await mediator.Send(new GetGradesQuery());
                
                if (!result.IsError)
                {
                    return Results.Ok(result.Value);
                }
                
                return Results.BadRequest(new 
                { 
                    Success = false, 
                    Errors = result.Errors.Select(e => new 
                    { 
                        Code = e.Code, 
                        Description = e.Description 
                    }).ToList()
                });
            })
            .WithName("GetCoreGrades");

            // Test Grades - With Cache Info
            group.MapGet("/grades/cache-test", async (IMediator mediator, ILogger<Program> logger) =>
            {
                logger.LogInformation("=== FIRST CALL - Should fetch from API ===");
                var stopwatch1 = System.Diagnostics.Stopwatch.StartNew();
                var result1 = await mediator.Send(new GetGradesQuery());
                stopwatch1.Stop();

                logger.LogInformation("=== SECOND CALL - Should come from cache ===");
                var stopwatch2 = System.Diagnostics.Stopwatch.StartNew();
                var result2 = await mediator.Send(new GetGradesQuery());
                stopwatch2.Stop();

                if (result1.IsError)
                {
                    return Results.BadRequest(new 
                    { 
                        Success = false, 
                        Errors = result1.Errors.Select(e => new 
                        { 
                            Code = e.Code, 
                            Description = e.Description 
                        }).ToList()
                    });
                }

                return Results.Ok(new
                {
                    Success = true,
                    FirstCallMs = stopwatch1.ElapsedMilliseconds,
                    SecondCallMs = stopwatch2.ElapsedMilliseconds,
                    CacheWorking = stopwatch2.ElapsedMilliseconds < stopwatch1.ElapsedMilliseconds,
                    GradeCount = result1.Value.Count
                });
            })
            .WithName("TestGradesCaching");

            // Test Filtering (like Blazor would do)
            group.MapGet("/managers", async (IMediator mediator) =>
            {
                var usersResult = await mediator.Send(new GetUsersQuery());
                var gradesResult = await mediator.Send(new GetGradesQuery());

                if (usersResult.IsError || gradesResult.IsError)
                {
                    return Results.BadRequest(new { Success = false, Message = "Failed to fetch data" });
                }

                // Filter for managers (like Blazor would do)
                var managerGrade = gradesResult.Value.FirstOrDefault(g => g.Name.Contains("Manager"));
                var managers = usersResult.Value.Where(u => u.GradeId == managerGrade?.Id).ToList();

                return Results.Ok(new
                {
                    Success = true,
                    TotalUsers = usersResult.Value.Count,
                    ManagerGradeName = managerGrade?.Name,
                    ManagerCount = managers.Count,
                    Managers = managers.Select(m => new { m.Id, m.FullName, m.GradeId })
                });
            })
            .WithName("FilterManagers");

            return app;
        }
    }
}

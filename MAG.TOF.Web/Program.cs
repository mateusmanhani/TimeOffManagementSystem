using ErrorOr;
using MAG.TOF.Application.Commands.CreateRequests;
using MAG.TOF.Application.Commands.DeleteRequest;
using MAG.TOF.Application.Commands.RecallRequest;
using MAG.TOF.Application.Commands.UpdateRequest;
using MAG.TOF.Application.Interfaces;
using MAG.TOF.Application.Queries.GetUserRequests;
using MAG.TOF.Domain.Services;
using MAG.TOF.Infrastructure.Data;
using MAG.TOF.Infrastructure.Repositories;
using MAG.TOF.Infrastructure.Services;
using MAG.TOF.Web.Components;
using MAG.TOF.Web.Components.Account;
using MAG.TOF.Web.Data;
using MediatR;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore;
using NLog;
using NLog.Web;

// Initialize NLog early to catch startup errors
var logger = LogManager.Setup()
    .LoadConfigurationFromFile("nlog.config")
    .GetCurrentClassLogger();

logger.Info("Application starting up");

try
{
    var builder = WebApplication.CreateBuilder(args);

    // NLog: Setup NLog for Dependency Injection
    builder.Logging.ClearProviders();
    builder.Host.UseNLog();

    // Add services to the container.
    builder.Services.AddRazorComponents()
        .AddInteractiveServerComponents();

    builder.Services.AddCascadingAuthenticationState();
    builder.Services.AddScoped<IdentityRedirectManager>();
    builder.Services.AddScoped<AuthenticationStateProvider, IdentityRevalidatingAuthenticationStateProvider>();

    builder.Services.AddAuthentication(options =>
        {
            options.DefaultScheme = IdentityConstants.ApplicationScheme;
            options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
        })
        .AddIdentityCookies();

    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
    
    // Register ApplicationDbContext (for Identity)
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseSqlServer(connectionString));
    
    // Register TofDbContext (for business domain)
    builder.Services.AddDbContext<TofDbContext>(options =>
        options.UseSqlServer(connectionString));

    // Register Repository
    builder.Services.AddScoped<IRequestRepository, RequestRepository>();

    //  Register Domain Services
    builder.Services.AddScoped<RequestValidationService>();

    // Register HttpClient for CORE API
    builder.Services.AddHttpClient<ICoreApiClient, CoreApiService>(client =>
    {
        var baseUrl = builder.Configuration["CoreApi:BaseUrl"]
            ?? throw new InvalidOperationException("CoreApi:BaseUrl not configured in appsetting.json");

        client.BaseAddress = new Uri(baseUrl);
        client.Timeout = TimeSpan.FromSeconds(builder.Configuration.GetValue<int>("CoreApi:Timeout", 30));
        client.DefaultRequestHeaders.Add("Accept", "application/json");
    });

    //  Register In-Memory Cache
    builder.Services.AddMemoryCache();
    builder.Services.AddScoped<ICacheService, InMemoryCacheService>();

    // Register MediatR
    builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly
    (typeof(CreateRequestCommand).Assembly));


    builder.Services.AddDatabaseDeveloperPageExceptionFilter();

    builder.Services.AddIdentityCore<ApplicationUser>(options =>
        {
            options.SignIn.RequireConfirmedAccount = true;
            options.Stores.SchemaVersion = IdentitySchemaVersions.Version3;
        })
        .AddEntityFrameworkStores<ApplicationDbContext>()
        .AddSignInManager()
        .AddDefaultTokenProviders();

    builder.Services.AddSingleton<IEmailSender<ApplicationUser>, IdentityNoOpEmailSender>();

    var app = builder.Build();

    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.UseMigrationsEndPoint();
    }
    else
    {
        app.UseExceptionHandler("/Error", createScopeForErrors: true);
        // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
        app.UseHsts();
    }
    app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
    app.UseHttpsRedirection();

    app.UseAntiforgery();

    app.MapStaticAssets();
    app.MapRazorComponents<App>()
        .AddInteractiveServerRenderMode();

    // Add additional endpoints required by the Identity /Account Razor components.
    app.MapAdditionalIdentityEndpoints();

    // Create testing endpoints - Disable antiforgery for testing
    app.MapPost("/api/test/create-request", async (
        CreateRequestCommand command,
        IMediator mediator) =>
    {
        var result = await mediator.Send(command);

        return result.Match(
            id => Results.Ok(new { RequestId = id }),
            errors => Results.BadRequest(new { Errors = errors })
            );
    })
    .DisableAntiforgery(); // disable antiforgery validation for testing

    // Test endpoint to getRequests by UserId
    app.MapGet("/api/test/get-requests-by-user/{userId}", async (
        int userId,
        IMediator mediator) =>
    {
        var query = new GetRequestsByUserQuery(userId);
        var result = await mediator.Send(query);

        return result.Match(
            requests => Results.Ok(requests),
            errors => Results.BadRequest(new { Errors = errors })
            );
    }).DisableAntiforgery();

    // UpdateRequest test endpoint
    app.MapPut("/api/test/update-request", async (
        UpdateRequestCommand command,
        IMediator mediator) =>
    {
        var result = await mediator.Send(command);

        return result.Match(
            success => Results.Ok(new { Message = "Request updated successfully" }),
            errors => Results.BadRequest(new { Errors = errors })
        );
    }).DisableAntiforgery();

    // Delete - uses route parameter
    app.MapDelete("/api/test/delete-request/{requestId}", async (
        int requestId,
        IMediator mediator) =>
    {
        var command = new DeleteRequestCommand(requestId);
        var result = await mediator.Send(command);

        return result.Match(
            success => Results.Ok(new { Message = "Request Deleted Successfully" }),
            errors => Results.BadRequest(new { Errors = errors })
        );
    }).DisableAntiforgery();

    // Recall - uses route parameter
    app.MapPatch("/api/test/recall-request/{requestId}", async (
        int requestId,
        IMediator mediator) =>
    {
        var command = new RecallRequestCommand(requestId);
        var result = await mediator.Send(command);

        return result.Match(
            success => Results.Ok(new { Message = "Request Recalled Successfully, Status changed to Recalled." }),
            errors => Results.BadRequest(new { Errors = errors })
        );
    }).DisableAntiforgery();


    //  Test CORE API - Get Users
    app.MapGet("/api/test/core-users", async (ICoreApiClient coreApiClient, ILogger<Program> logger) =>
    {
        try
        {
            logger.LogInformation("TEST: Calling CoreApiService.GetUsersAsync()");
            var users = await coreApiClient.GetUsersAsync();
            return Results.Ok(new
            {
                Success = true,
                Message = "Successfully fetched Users from core API",
                Count = users.Count,
                Users = users
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "TEST: Failed to fetch users from CORE API");
            return Results.BadRequest(new
            {
                Success = false,
                Error = ex.Message
            });
        }
    }).DisableAntiforgery();

    // Test CORE API - Get Departments
    app.MapGet("/api/test/core-departments", async (ICoreApiClient coreApiClient, ILogger<Program> logger) =>
    {
        try
        {
            logger.LogInformation("TEST: Calling CoreApiService.GetDepartmentsAsync()");
            var departments = await coreApiClient.GetDepartmentsAsync();
            return Results.Ok(new
            {
                Success = true,
                Message = "Successfully fetched departments from core API",
                Count = departments.Count,
                Departments = departments
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "TEST: Failed to fetch departments from CORE API");
            return Results.BadRequest(new
            {
                Success = false,
                Error = ex.Message
            });
        }
    }).DisableAntiforgery();

    // Test CORE API - Get Grades
    app.MapGet("/api/test/core-grades", async (ICoreApiClient coreApiClient, ILogger<Program> logger) =>
    {
        try
        {
            logger.LogInformation("TEST: Calling CoreApiService.GetGradesAsync()");
            var grades = await coreApiClient.GetGradesAsync();
            return Results.Ok(new
            {
                Success = true,
                Message = "Successfully fetched grades from core API",
                Count = grades.Count,
                Grades = grades
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "TEST: Failed to fetch grades from CORE API");
            return Results.BadRequest(new
            {
                Success = false,
                Error = ex.Message
            });
        }
    }).DisableAntiforgery();

    //Test Caching
    app.MapGet("/api/test/cache-test", async (ICacheService cacheService, ICoreApiClient coreApiClient) =>
    {
        var cacheKey = "test_users";

        // First call - should fetch from API
        var users1 = await cacheService.GetOrCreateAsync(
            cacheKey,
            async () => await coreApiClient.GetUsersAsync(),
            TimeSpan.FromMinutes(5)
        );

        // Second call - should come from cache
        var users2 = await cacheService.GetOrCreateAsync(
            cacheKey,
            async () => await coreApiClient.GetUsersAsync(),
            TimeSpan.FromMinutes(5)
        );

        return Results.Ok(new
        {
            Message = "Cache working!",
            FirstCallCount = users1.Count,
            SecondCallCount = users2.Count,
            Note = "Check logs - second call should not hit API"
        });
    }).DisableAntiforgery();



    logger.Info("Application started successfully");

    app.Run();
}
catch (Exception ex)
{
    // NLog: catch setup errors
    logger.Error(ex, "Application failed to start");
    throw;
}
finally
{
    // Ensure to flush and stop internal timers/threads before application-exit
    logger.Info("Application shutting down");
    LogManager.Shutdown();
}

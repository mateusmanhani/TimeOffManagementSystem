using MAG.TOF.Application.Commands.CreateRequests;
using MAG.TOF.Application.Commands.DeleteRequest;
using MAG.TOF.Application.Commands.RecallRequest;
using MAG.TOF.Application.Commands.UpdateRequest;
using MAG.TOF.Application.Interfaces;
using MAG.TOF.Application.Queries.GetUserRequests;
using MAG.TOF.Domain.Services;
using MAG.TOF.Infrastructure.Data;
using MAG.TOF.Infrastructure.Repositories;
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
    builder.Services.AddScoped<ITofRepository, TofRepository>();

    //  Register Domain Services
    builder.Services.AddScoped<RequestValidationService>();

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

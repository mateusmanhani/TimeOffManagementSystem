using MAG.TOF.Application.CQRS.Commands.CreateRequest;
using MAG.TOF.Application.Interfaces;
using MAG.TOF.Application.Validation;
using MAG.TOF.Domain.Services;
using MAG.TOF.Infrastructure.Data;
using MAG.TOF.Infrastructure.Repositories;
using MAG.TOF.Infrastructure.Services;
using MAG.TOF.Web.Components;
using MAG.TOF.Web.Components.Account;
using MAG.TOF.Web.Data;
using MAG.TOF.Web.Endpoints;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MudBlazor.Services;
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

    // Register Validation Services
    builder.Services.AddScoped<ExternalDataValidator>();

    builder.Services.AddScoped<RequestValidationService>();

    // Register MudBlazor Services
    builder.Services.AddMudServices();

    // Register HttpClient for CORE API
    builder.Services.AddHttpClient<ICoreApiService, CoreApiService>(client =>
    {
        var baseUrl = builder.Configuration["CoreApi:BaseUrl"]
            ?? throw new InvalidOperationException("CoreApi:BaseUrl not configured in appsetting.json");

        client.BaseAddress = new Uri(baseUrl);
        client.Timeout = TimeSpan.FromSeconds(builder.Configuration.GetValue<int>("CoreApi:Timeout", 30));
        client.DefaultRequestHeaders.Add("Accept", "application/json");
    });

    //  Register In-Memory Cache
    builder.Services.AddMemoryCache();
    builder.Services.AddSingleton<ICacheService, InMemoryCacheService>();

    // Register External Data Cache
    builder.Services.AddScoped<IExternalDataCache, ExternalDataCache>(); // Cannot be Singleton unless ICoreApiService and ICacheService are singleton as well

    // Register MediatR
    builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly
    (typeof(CreateRequestCommand).Assembly));

    // ==================== Swagger/OpenAPI Configuration ====================
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();
    
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
        
        // Enable Swagger in Development
        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "MAG TOF API V1");
            options.RoutePrefix = "swagger";
            options.DocumentTitle = "MAG TOF API Documentation";
        });
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

    // ==================== TEST ENDPOINTS (Organized via Extension Methods) ====================
    
    // Map all CORE API test endpoints (users, departments, grades with caching tests)
    app.MapCoreApiTestEndpoints();
    
    // Map all Request CRUD test endpoints (create, read, update, delete, recall)
    app.MapRequestTestEndpoints();
    
    // ==================== End of Test Endpoints ====================

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

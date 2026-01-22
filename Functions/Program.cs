using MAG.TOF.Application.Interfaces;
using MAG.TOF.Infrastructure.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = FunctionsApplication.CreateBuilder(args);

// regiser infrastructure email sender - MailKitEmailSender for local testing, NoOpEmailSender for production for now
builder.Services.AddSingleton<IEmailSender, NoOpEmailSender>();

builder.ConfigureFunctionsWebApplication();

builder.Services
    .AddApplicationInsightsTelemetryWorkerService()
    .ConfigureFunctionsApplicationInsights();

builder.Build().Run();

using Infrastructure.Startup;

var builder = WebApplication.CreateBuilder(args);
builder.Services.ConfigureDependencies()
    .ConfigureSettings()
    .ConfigureDatabase(builder.Configuration)
    .ConfigureWeb()
    .ConfigureApi()
    .ConfigureCookies()
    .ConfigureCoors()
    .ConfigurePolicies();

builder.Host.ConfigureSerilog(builder.Configuration);
builder.Logging.ConfigureLogging();


var app = builder.Build();

app.ConfigureWebApp()
    .ConfigureSwagger()
    .Run();

public partial class Program;

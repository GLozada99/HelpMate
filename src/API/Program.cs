using Infrastructure.Startup;

var builder = WebApplication.CreateBuilder(args);
builder.Services.ConfigureDependencies();
builder.Services.ConfigureSettings();
builder.Services.ConfigureDatabase();

builder.Host.ConfigureSerilog();
builder.Logging.ConfigureLogging();


builder.Services.ConfigureWeb();
builder.Services.ConfigureApi();
builder.Services.ConfigureCookies();
builder.Services.ConfigureCoors();
builder.Services.ConfigurePolicies();


var app = builder.Build();

app.ConfigureWebApp();
app.ConfigureSwagger();


app.Run();

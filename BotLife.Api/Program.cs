using BotLife.Api;
using MassTransit;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Build configuration
var configurationBuilder = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

if (builder.Environment.IsDevelopment())
{
    configurationBuilder.AddJsonFile($"appsettings.Development.json", optional: true);
    //configurationBuilder.AddUserSecrets<Program>(optional: true, reloadOnChange: true);
}

var configuration = configurationBuilder.Build();

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCors();

// Add dependencies
builder.Services.AddBotLifeServices(configuration);

builder.Host.UseSerilog((context, loggerConfig) =>
    loggerConfig.ReadFrom.Configuration(context.Configuration));

// Masstransit
builder.Services.AddMassTransit(x =>
{
    x.SetKebabCaseEndpointNameFormatter();
    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host("localhost", "/",
            h =>
            {
                h.Username("botlife");
                h.Password("pwd123");
            });
        cfg.ConfigureEndpoints(context);
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors(b => b
    .AllowAnyOrigin()
    .AllowAnyHeader()
    .AllowAnyMethod()
);

app.UseHttpsRedirection();

app.MapBotLifeApi();

app.Run();

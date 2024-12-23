﻿using BotLife.Consumer;
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
    configurationBuilder.AddUserSecrets<Program>(optional: true, reloadOnChange: true);
}

var configuration = configurationBuilder.Build();

// Add dependencies
builder.Services.AddBotLifeServices(configuration);

builder.Host.UseSerilog((context, loggerConfig) =>
    loggerConfig.ReadFrom.Configuration(context.Configuration));

builder.Services.AddMassTransit(x =>
{
    x.SetKebabCaseEndpointNameFormatter();
    
    var assembly = typeof(Program).Assembly;
    x.AddConsumers(assembly);
    x.AddActivities(assembly);
    
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

app.Run();
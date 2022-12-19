using Microsoft.AspNetCore.Mvc;
using RemoteProcessManager;
using RemoteProcessManager.Enums;
using RemoteProcessManager.Managers;
using RemoteProcessManager.MessageBroker;
using RemoteProcessManager.MessageBroker.Redis;
using RemoteProcessManager.Models;

var builder = WebApplication.CreateBuilder();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var isFromArgs = args.Length >= 4;
var jsonSettings = builder.Configuration.GetSection(nameof(Settings)).Get<Settings>();
//CommandLineParser - nuget package
var settings = new Settings
{
    AgentMode = isFromArgs ? (ModeType)int.Parse(args[0]) : jsonSettings!.AgentMode,
    AgentName = isFromArgs ? args[1] : jsonSettings!.AgentName,
    MessageBrokerUrl = isFromArgs ? args[2] : jsonSettings!.MessageBrokerUrl,
    HttpPort = isFromArgs ? int.Parse(args[3]) : jsonSettings!.HttpPort,
    ProcessFullName = args.Length >= 5 ? args[4] : jsonSettings!.ProcessFullName,
    ProcessArguments = args.Length >= 6 ? args[5] : jsonSettings!.ProcessArguments
};

builder.Services.AddSingleton(settings);
builder.Services.AddSingleton<IProducer, RedisProducer>();
builder.Services.AddSingleton<IConsumer, RedisConsumer>();
builder.Services.AddSingleton<IProcessManager, ProcessManager>();
builder.Services.AddHostedService<Worker>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/processManager", ([FromServices] Settings appSettings) => $"Running in mode {appSettings.AgentMode:G}");

app.Run($"http://*:{settings.HttpPort}");
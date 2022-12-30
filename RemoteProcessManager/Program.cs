using CommandLine;
using Microsoft.AspNetCore.Mvc;
using RemoteProcessManager;
using RemoteProcessManager.Enums;
using RemoteProcessManager.Logic;
using RemoteProcessManager.Logic.Interfaces;
using RemoteProcessManager.MessageBroker;
using RemoteProcessManager.MessageBroker.Redis;
using RemoteProcessManager.Models;
using RemoteProcessManager.Services;
using RemoteProcessManager.Services.Interfaces;

var builder = WebApplication.CreateBuilder();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var arguments = Parser.Default.ParseArguments<Settings>(args);
if (ValidateArguments(arguments)) return;

var settings = arguments.Value;

builder.Services.AddSingleton(settings!);
builder.Services.AddSingleton<IProducer, RedisProducer>();
builder.Services.AddSingleton<IConsumer, RedisConsumer>();
builder.Services.AddSingleton<IProcessService, ProcessService>();
builder.Services.AddSingleton(typeof(ICacheService<>), typeof(TempFileService<>));

if (settings.AgentMode is ModeType.AgentProxy)
    builder.Services.AddSingleton<IAgent, ProxyAgent>();
else
    builder.Services.AddSingleton<IAgent, Agent>();

builder.Services.AddHostedService<Worker>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/processService", ([FromServices] Settings appSettings) => $"Worker running as {appSettings.AgentMode:G}");

app.Run($"http://*:{settings.HttpPort}");

bool ValidateArguments(ParserResult<Settings> parserResult)
{
    if (parserResult.Errors.Any()) return true;
    if (parserResult.Value.AgentMode is ModeType.AgentProxy &&
        string.IsNullOrEmpty(parserResult.Value.ProcessFullName))
        throw new ArgumentException("AgentProxy must have a process-name argument");
    return false;
}
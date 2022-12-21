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

var result = Parser.Default.ParseArguments<Settings>(args);
if (result.Errors.Any()) return;
var settings = result.Value;

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

app.MapGet("/processService", ([FromServices] Settings appSettings) => $"Running in mode {appSettings.AgentMode:G}");

app.Run($"http://*:{settings.HttpPort}");

/*
 * 1. agent need to start from temp file
   2. if remote process crashes and not canceled then kill agent process for watch dog to started again
   3. after watch dog started again agent check if remote process is running than connect to it and subscribe to stream output again
 */
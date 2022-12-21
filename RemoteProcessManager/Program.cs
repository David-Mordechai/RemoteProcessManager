using CommandLine;
using Microsoft.AspNetCore.Mvc;
using RemoteProcessManager;
using RemoteProcessManager.Managers;
using RemoteProcessManager.Managers.Interfaces;
using RemoteProcessManager.MessageBroker;
using RemoteProcessManager.MessageBroker.Redis;
using RemoteProcessManager.Models;

var builder = WebApplication.CreateBuilder();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var result = Parser.Default.ParseArguments<Settings>(args);
if (result.Errors.Any()) return;
var settings = result.Value;

builder.Services.AddSingleton(settings!);
builder.Services.AddSingleton<IProducer, RedisProducer>();
builder.Services.AddSingleton<IConsumer, RedisConsumer>();
builder.Services.AddSingleton<IProcessManager, ProcessManager>();
builder.Services.AddSingleton<ICacheManager, TempFileManager>();
builder.Services.AddHostedService<Worker>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/processManager", ([FromServices] Settings appSettings) => $"Running in mode {appSettings.AgentMode:G}");

app.Run($"http://*:{settings.HttpPort}");
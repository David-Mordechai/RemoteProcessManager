using CommandLine;
using Microsoft.AspNetCore.Mvc;
using RemoteProcessManager;
using RemoteProcessManager.Enums;
using RemoteProcessManager.Managers;
using RemoteProcessManager.Managers.Interfaces;
using RemoteProcessManager.MessageBroker;
using RemoteProcessManager.MessageBroker.Redis;
using RemoteProcessManager.Models;

var builder = WebApplication.CreateBuilder();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var jsonSettings = builder.Configuration.GetSection(nameof(Settings)).Get<Settings>();

var result = Parser.Default.ParseArguments<Options>(args);
if (result.Errors.Any())
    return;

var isFromArgs = args.Length >= 4;
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

public class Options
{
    [Option('t', "agent-type", Required = true, HelpText = "Set Agent type, 1 = Agent, 2 = ProxyAgent")]
    public ModeType? AgentType { get; set; }

    [Option('n', "agent-name", HelpText = "Set Agent Name, for example video1, this name will be use for creating message broker topics")]
    public string? AgentName { get; set; }

    [Option('u', "messageBroker-url", HelpText = "Set MessageBroker Url, for example, redis url 127.0.0.1:6379")]
    public string? MessageBrokerUrl { get; set; }

    [Option('p', "http-port", HelpText = "Set Http Port, port for rest api")]
    public ulong? HttpPort { get; set; }
}
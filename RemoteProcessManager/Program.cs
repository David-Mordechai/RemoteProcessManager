using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting.WindowsServices;
using RemoteProcessManager;
using RemoteProcessManager.Enums;
using RemoteProcessManager.MessageBroker;
using RemoteProcessManager.MessageBroker.Redis;

var builder = WebApplication.CreateBuilder(new WebApplicationOptions
{
    Args = args,
    ContentRootPath = WindowsServiceHelpers.IsWindowsService() ? AppContext.BaseDirectory : default,
    ApplicationName = Process.GetCurrentProcess().ProcessName
});
builder.Host.UseWindowsService();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var isFromArgs = args.Length >= 5;
var jsonSettings = builder.Configuration.GetSection(nameof(Settings)).Get<Settings>();
var settings = new Settings
{
    AgentMode = isFromArgs ? (ModeType)int.Parse(args[0]) : jsonSettings!.AgentMode,
    HttpPort = isFromArgs ? int.Parse(args[1]) : jsonSettings!.HttpPort,
    MessageBrokerUrl = isFromArgs ? args[2] : jsonSettings!.MessageBrokerUrl,
    ProcessTopic = isFromArgs ? args[3] : jsonSettings!.ProcessTopic,
    StreamTopic = isFromArgs ? args[4] : jsonSettings!.StreamTopic,
    ProcessFullName = args.Length == 6 ? args[5] : jsonSettings!.ProcessFullName
};

builder.Services.AddSingleton(settings);
builder.Services.AddSingleton<IProducer, RedisProducer>();
builder.Services.AddSingleton<IConsumer, RedisConsumer>();
builder.Services.AddHostedService<Worker>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/processManager", ([FromServices] Settings appSettings) => $"Running in mode {appSettings.AgentMode:G}");

app.Run($"http://*:{settings.HttpPort}");
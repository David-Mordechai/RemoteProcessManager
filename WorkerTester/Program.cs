using WorkerTester;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        var settings = new Settings {Arguments = args};
        services.AddSingleton(settings);
        services.AddHostedService<Worker>();
    })
    .Build();

host.Run();


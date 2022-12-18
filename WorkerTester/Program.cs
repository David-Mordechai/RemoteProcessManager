using WorkerTester;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services.AddHostedService<Worker>();
    })
    .Build();

if (args.Length > 0)
{
    for (var i = 0; i < args.Length; i++)
    {
        Console.WriteLine($"Arg {i + 1}: {args[i]}");
    }
}
else
{
    Console.WriteLine("No args was passed...");
}

await host.RunAsync();

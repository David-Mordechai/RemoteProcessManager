namespace WorkerTester
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly Settings _settings;

        public Worker(ILogger<Worker> logger, Settings settings)
        {
            _logger = logger;
            _settings = settings;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if (_settings.Arguments?.Length > 0)
                for (var i = 0; i < _settings.Arguments.Length; i++)
                    Console.WriteLine($"Arg {i + 1}: {_settings.Arguments[i]}");
            else
                Console.WriteLine("No args was passed...");

            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                await Task.Delay(1000, stoppingToken);
            }
        }
    }
}
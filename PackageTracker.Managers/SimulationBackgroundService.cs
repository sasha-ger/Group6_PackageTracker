using PackageTracker.Engines;

namespace PackageTracker.Managers;

// Runs the simulation on a fixed interval, creating a fresh DI scope each tick to satisfy scoped dependencies.
public class SimulationBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly TimeSpan _interval = TimeSpan.FromSeconds(30);

    public SimulationBackgroundService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using var scope = _serviceProvider.CreateScope();
            var engine = scope.ServiceProvider.GetRequiredService<ISimulationEngine>();
            await engine.TickAsync();
            await Task.Delay(_interval, stoppingToken);
        }
    }
}

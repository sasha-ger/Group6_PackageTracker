namespace PackageTracker.Engines;

public interface ISimulationEngine
{
    Task TickAsync();
}

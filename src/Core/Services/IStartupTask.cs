namespace Core.Services;

public interface IStartupTask
{
    Task RunAsync(CancellationToken ct = default);
}
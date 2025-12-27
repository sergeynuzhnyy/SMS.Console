using Core.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Repositories;

public class SmsDbInitializer : IStartupTask
{
    private static readonly SemaphoreSlim _lock = new(1, 1);
    private static bool _initialized = false;
    private readonly SmsDbContext _dbContext;
    private readonly ILogger<SmsDbInitializer> _logger;

    public SmsDbInitializer(SmsDbContext dbContext, ILogger<SmsDbInitializer> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task RunAsync(CancellationToken ct = default)
    {
        await _lock.WaitAsync(ct);
        try
        {
            if (_initialized)
            {
                return;
            }

            _logger.LogDebug($"[{nameof(SmsDbInitializer)}] Initializing SmsDb");
            try
            {
                await _dbContext.Database.MigrateAsync(cancellationToken: ct);
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, ex.Message);
                throw new Exception($"An error occurred while migrating the database: {ex.Message}", ex);
            }
            _logger.LogDebug($"[{nameof(SmsDbInitializer)}] SmsDb initialized");

            _initialized = true;
        }
        finally
        {
            _lock.Release();
        }
    }
}
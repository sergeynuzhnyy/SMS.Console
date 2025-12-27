using Contracts.Models;
using Contracts.Models.Cafe;
using Core.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Repositories;

public class SmsRepository : ISmsRepository
{
    private SmsDbContext _dbContext;
    private ILogger<SmsRepository> _logger;

    public SmsRepository(SmsDbContext dbContext, ILogger<SmsRepository> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<DbResult[]> GetAllDbResultAsync(CancellationToken cancellationToken = default)
    {
        // Можно обернуть в try/catch, чтобы более точно возвращать ошибку
        return await _dbContext.DbResults.ToArrayAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
    }
    
    public async Task<bool> InsertDbResultAsync(DbResult dbResult, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Inserting dbResult");
        // Перед инсёртом можно проверить объект
        // И обернуть в try/catch, чтобы более точно возвращать ошибку
        _dbContext.DbResults.Add(dbResult);
        var result = await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false) > 0;
        _logger.LogInformation("Inserted dbResult");
        return result;
    }
}
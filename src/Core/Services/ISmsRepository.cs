using Contracts.Models;
using Contracts.Models.Cafe;

namespace Core.Services;

public interface ISmsRepository
{
    Task<DbResult[]> GetAllDbResultAsync(CancellationToken cancellationToken = default);
    Task<bool> InsertDbResultAsync(DbResult dbResult, CancellationToken cancellationToken = default);
}
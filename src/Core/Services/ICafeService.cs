using Contracts.Models.Cafe;

namespace Core.Services;

public interface ICafeService
{
    Task<Menu> GetMenu(MenuParameters parameters, CancellationToken cancellationToken);
    Task<bool> SendOrder(MenuItem[] menuItems, CancellationToken cancellationToken);
}
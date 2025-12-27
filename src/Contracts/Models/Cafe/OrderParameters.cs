namespace Contracts.Models.Cafe;

public record OrderParameters
{
    public Guid OrderId { get; init; }
    public MenuItem[] MenuItems { get; init; }
}
namespace Contracts.Models.Cafe;

public record Menu
{
    public MenuItem[] MenuItems { get; init; }
}
namespace Contracts.Models.Cafe;

public record MenuItem
{
    public string Id { get; init; } = null!;
    public string? Article { get; init; }
    public string? Name { get; init; }
    public decimal? Price { get; init; }
    public bool? IsWeighted { get; init; }
    public string? FullPath { get; init; }
    public string[]? Barcodes { get; init; }
    public decimal? Quantity { get; init; }
}
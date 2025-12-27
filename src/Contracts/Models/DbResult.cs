namespace Contracts.Models;

public record DbResult
{
    public Guid Id { get; init; }
    public string Command  { get; init; }
    public bool IsSuccess { get; init; }
    public string Message { get; init; }
    public DateTimeOffset CreatedOn { get; init; }
}
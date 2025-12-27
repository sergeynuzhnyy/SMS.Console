namespace Contracts.Models.Api;

public record ApiRequest<T>
{
    public string Command { get; init; }
    public T CommandParameters { get; init; }
}
namespace Contracts.Models.Api;

public record ApiResponse
{
    public string Command { get; init; }
    public bool Success { get; init; }
    public string ErrorMessage { get; init; }
}

public record ApiResponse<T> : ApiResponse
{
    public T Data { get; init; }
}
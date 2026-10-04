namespace CodeBase.API.Contracts;

public sealed record ApiResponse<T>(
    T Value,
    string? TraceId,
    DateTimeOffset TimestampUtc);

public static class ApiResponseFactory
{
    public static ApiResponse<T> Success<T>(T value, string? traceId = null) =>
        new(value, traceId, DateTimeOffset.UtcNow);
}

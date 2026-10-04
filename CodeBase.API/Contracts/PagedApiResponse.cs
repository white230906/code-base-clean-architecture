namespace CodeBase.API.Contracts;

public sealed record PagedApiResponse<T>(
    IReadOnlyList<T> Items,
    int PageIndex,
    int PageSize,
    int TotalCount,
    int TotalPages,
    bool HasPreviousPage,
    bool HasNextPage,
    string? TraceId,
    DateTimeOffset TimestampUtc);

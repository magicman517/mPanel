namespace mPanel.API.Core.Common;

internal sealed record PagedResponse<T>
{
    public required IReadOnlyCollection<T> Items { get; init; }
    public required int TotalCount { get; init; }
    public required int Page { get; init; }
    public required int PageSize { get; init; }
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
}

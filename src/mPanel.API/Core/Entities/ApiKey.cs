using mPanel.API.Core.Common;

namespace mPanel.API.Core.Entities;

public class ApiKey : BaseEntity
{
    public required Guid UserId { get; set; }

    public string? Name { get; set; }

    public required string Prefix { get; set; }
    public required string Hash { get; set; }

    public DateOnly? ExpiresAt { get; set; }

    public bool IsActive => ExpiresAt == null || ExpiresAt > DateOnly.FromDateTime(DateTime.UtcNow);

    public ApplicationUser User { get; set; } = null!;
}
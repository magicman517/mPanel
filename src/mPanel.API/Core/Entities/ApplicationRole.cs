using Microsoft.AspNetCore.Identity;

namespace mPanel.API.Core.Entities;

public sealed class ApplicationRole : IdentityRole<Guid>
{
    public override Guid Id { get; set; } = Guid.CreateVersion7();
}
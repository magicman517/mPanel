using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Identity;

namespace mPanel.API.Core.Entities;

public sealed class ApplicationUser : IdentityUser<Guid>
{
    public override Guid Id { get; set; } = Guid.CreateVersion7();

    public required string AvatarUrl { get; set; }

    public ICollection<ApiKey> ApiKeys { get; set; } = new List<ApiKey>();

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public static string GenerateAvatarUrl(string email)
    {
        const string baseUrl = "https://www.gravatar.com/avatar/";
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(email));
        var hex = Convert.ToHexString(hash).ToLower();
        return $"{baseUrl}{hex}?s=200&d=robohash&r=pg";
    }
}
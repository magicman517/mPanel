using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using StackExchange.Redis;

namespace mPanel.API.Infrastructure.Identity;

public class RedisTicketStore(IDatabase db) : ITicketStore
{
    private const string SessionsKeyPrefix = "sessions:";
    private const string SessionKeyPrefix = "session:";

    private readonly TicketSerializer _serializer = TicketSerializer.Default;

    public async Task<string> StoreAsync(AuthenticationTicket ticket)
    {
        var sessionId = Guid.NewGuid().ToString("N");
        var userId = ticket.Principal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value ?? "unknown";

        await RenewAsync(sessionId, ticket);
        await db.SetAddAsync(SessionsKeyPrefix + userId, sessionId);
        await ExtendUserIndexExpiryAsync(userId, ticket);

        return sessionId;
    }

    public async Task RenewAsync(string key, AuthenticationTicket ticket)
    {
        var expiry = ticket.Properties.ExpiresUtc is { } exp
            ? exp - DateTimeOffset.UtcNow
            : TimeSpan.Zero;

        if (expiry <= TimeSpan.Zero)
        {
            await db.KeyDeleteAsync(SessionKeyPrefix + key);
            await RemoveFromUserIndexAsync(key);
            return;
        }

        var bytes = _serializer.Serialize(ticket);
        await db.StringSetAsync(SessionKeyPrefix + key, bytes, expiry);
    }

    public async Task<AuthenticationTicket?> RetrieveAsync(string key)
    {
        var bytes = await db.StringGetAsync(SessionKeyPrefix + key);
        return bytes.IsNullOrEmpty
            ? null
            : _serializer.Deserialize(bytes!);
    }

    public async Task RemoveAsync(string key)
    {
        await RemoveFromUserIndexAsync(key);
        await db.KeyDeleteAsync(SessionKeyPrefix + key);
    }

    private async Task RemoveFromUserIndexAsync(string key)
    {
        var bytes = await db.StringGetAsync(SessionKeyPrefix + key);
        if (bytes.IsNullOrEmpty) return;

        var ticket = _serializer.Deserialize(bytes!);
        var userId = ticket?.Principal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

        if (userId is not null)
        {
            await db.SetRemoveAsync(SessionsKeyPrefix + userId, key);
        }
    }

    private async Task ExtendUserIndexExpiryAsync(string userId, AuthenticationTicket ticket)
    {
        var expiry = ticket.Properties.ExpiresUtc is { } exp
            ? exp - DateTimeOffset.UtcNow
            : TimeSpan.Zero;

        if (expiry <= TimeSpan.Zero) return;

        var setKey = SessionsKeyPrefix + userId;
        var currentTtl = await db.KeyTimeToLiveAsync(setKey);

        if (currentTtl is null || currentTtl < expiry)
            await db.KeyExpireAsync(setKey, expiry);
    }
}
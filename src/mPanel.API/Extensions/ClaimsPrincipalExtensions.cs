using System.Security.Claims;

namespace mPanel.API.Extensions;

public static class ClaimsPrincipalExtensions
{
    extension(ClaimsPrincipal principal)
    {
        public Guid? GetUserId()
        {
            var value = principal.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(value, out var id) ? id : null;
        }

        public Guid? GetNodeId()
        {
            // node and users use same claim for id
            // so we can just reuse existing extension
            return principal.GetUserId();
        }
    }
}

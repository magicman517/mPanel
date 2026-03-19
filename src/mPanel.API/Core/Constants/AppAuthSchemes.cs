using Microsoft.AspNetCore.Identity;

namespace mPanel.API.Core.Constants;

public static class AppAuthSchemes
{
    public static readonly string Cookie = IdentityConstants.ApplicationScheme;
    public const string ApiKey = "ApiKey";
    public const string NodeToken = "NodeToken";
}
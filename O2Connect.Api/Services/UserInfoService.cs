using O2Connect.Api.Exceptions;
using System.Security.Claims;

namespace O2Connect.Api.Services;

public interface IUserInfoService
{
    Task<IDictionary<string, object>> GetUserInfoAsync(ClaimsPrincipal user, CancellationToken ct);
}

public class UserInfoService : IUserInfoService
{
    private static readonly string[] UserInfoScopes = ["profile", "email", "phone", "address"];

    public Task<IDictionary<string, object>> GetUserInfoAsync(ClaimsPrincipal user, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        if (user?.Identity?.IsAuthenticated != true)
            throw OAuthException.FromInvalidToken("Unauthenticated user");

        var claims = user.Claims.ToList();

        if (claims.Count == 0)
            throw OAuthException.FromInvalidToken("No claims present");

        var sub = user.FindFirst("sub")?.Value;

        if (sub == null)
            throw OAuthException.FromInvalidToken("Missing sub claim");

        IDictionary<string, object> response = new Dictionary<string, object>(StringComparer.Ordinal);

        response["sub"] = sub;

        var scopes = claims.Where(c => c.Type == "scope" || c.Type == "scp")
                           .SelectMany(c => c.Value.Split(' ', StringSplitOptions.RemoveEmptyEntries))
                           .ToHashSet(StringComparer.OrdinalIgnoreCase);

        if (!scopes.Contains("openid"))
            throw OAuthException.FromInvalidToken("Missing openid scope");

        if (!scopes.Overlaps(UserInfoScopes))
            return Task.FromResult(response);

        var claimMap = claims.GroupBy(c => c.Type)
                             .ToDictionary(g => g.Key, g => g.Select(c => c.Value).ToList());

        if (scopes.Contains("profile"))
        {
            AddIfExists(response, claimMap, "name");
            AddIfExists(response, claimMap, "preferred_username");
            AddIfExists(response, claimMap, "family_name");
            AddIfExists(response, claimMap, "given_name");
            AddIfExists(response, claimMap, "middle_name");
            AddIfExists(response, claimMap, "nickname");
            AddIfExists(response, claimMap, "picture");
            AddIfExists(response, claimMap, "updated_at");
            AddIfExists(response, claimMap, "birthdate");
            AddIfExists(response, claimMap, "locale");
            AddIfExists(response, claimMap, "zoneinfo");
            AddIfExists(response, claimMap, "gender");
            AddIfExists(response, claimMap, "website");
        }

        if (scopes.Contains("email"))
        {
            AddIfExists(response, claimMap, "email");
            AddIfExists(response, claimMap, "email_verified");
        }

        if (scopes.Contains("phone"))
        {
            AddIfExists(response, claimMap, "phone_number");
            AddIfExists(response, claimMap, "phone_number_verified");
        }

        if (scopes.Contains("address"))
        {
            AddIfExists(response, claimMap, "address");
        }

        return Task.FromResult(response);
    }

    private static void AddIfExists(IDictionary<string, object> response,
                                    IDictionary<string, List<string>> claimMap,
                                    string claimType)
    {
        if (!claimMap.TryGetValue(claimType, out var values) || values.Count == 0)
            return;

        if (claimType == "updated_at" 
            && values.Count == 1 
            && long.TryParse(values[0], out var ts))
        {
            response[claimType] = ts;
            return;
        }

        if ((claimType == "email_verified" || claimType == "phone_number_verified")
            && values.Count == 1
            && bool.TryParse(values[0], out var b))
        {
            response[claimType] = b;
            return;
        }

        if (values.Count == 1)
            response[claimType] = values[0];
        else
            response[claimType] = values;
    }
}

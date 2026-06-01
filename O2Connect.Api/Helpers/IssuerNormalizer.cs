namespace O2Connect.Api.Helpers;

public static class IssuerNormalizer
{
    public static string Normalize(string issuer)
    {
        if (string.IsNullOrWhiteSpace(issuer))
            throw new InvalidOperationException("Issuer cannot be null or empty.");

        if (!Uri.TryCreate(issuer, UriKind.Absolute, out var uri))
            throw new InvalidOperationException($"Issuer must be an absolute URI: {issuer}");

        var builder = new UriBuilder(uri)
        {
            Path = string.Empty,
            Query = string.Empty,
            Fragment = string.Empty
        };

        var normalized = builder.Uri.ToString().TrimEnd('/');

        return normalized;
    }
}

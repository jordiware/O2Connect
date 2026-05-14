using O2Connect.Api.Exceptions;
using O2Connect.Api.Models;
using O2Connect.Api.Repositories;
using O2Connect.Dto.Requests;

namespace O2Connect.Api.Services.Validators;

public interface IClientValidator
{
    Task<ValidatedClient> ValidateAsync(TokenRequest request, CancellationToken ct);
}

public class ClientValidator : IClientValidator
{
    private readonly IClientRepository _clientRepository;

    public ClientValidator(IClientRepository clientRepository)
    {
        _clientRepository = clientRepository;
    }

    public async Task<ValidatedClient> ValidateAsync(TokenRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.ClientId))
            throw new OAuthException("invalid_request", "client_id is required");

        if (string.IsNullOrWhiteSpace(request.GrantType))
            throw new OAuthException("invalid_request", "grant_type is required");

        var client = await _clientRepository.GetByIdAsync(request.ClientId, ct);

        if (client == null)
            throw new OAuthException("invalid_client");

        if (client.RequiresSecret)
        {
            if (string.IsNullOrWhiteSpace(request.ClientSecret))
                throw new OAuthException("invalid_client");

            var valid = await _clientRepository.ValidateClientAsync(request.ClientId, request.ClientSecret, ct);

            if (!valid)
                throw new OAuthException("invalid_client");
        }

        if (!client.AllowedGrantTypes.Contains(request.GrantType))
            throw new OAuthException("unauthorized_client");

        var requestedScopes = ParseScopes(request.Scope);

        if (requestedScopes.Count > 0)
        {
            var allowed = new HashSet<string>(client.AllowedScopes, StringComparer.Ordinal);

            if (!requestedScopes.All(s => allowed.Contains(s)))
                throw new OAuthException("invalid_scope");
        }

        return new ValidatedClient
        {
            Client = client,
            RequestedScopes = requestedScopes
        };
    }

    private static IReadOnlyCollection<string> ParseScopes(string? scope)
    {
        if (string.IsNullOrWhiteSpace(scope))
            return Array.Empty<string>();

        return scope
            .Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Distinct(StringComparer.Ordinal)
            .ToArray();
    }
}

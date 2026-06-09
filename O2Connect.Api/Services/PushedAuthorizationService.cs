using O2Connect.Api.Crypto;
using O2Connect.Api.DataValidators;
using O2Connect.Api.Exceptions;
using O2Connect.Api.Models.Store;
using O2Connect.Api.Repositories;
using O2Connect.Dto.Requests;
using O2Connect.Dto.Responses;

namespace O2Connect.Api.Services;

public interface IPushedAuthorizationService
{
    Task<PushedAuthorizationResponse> HandleAsync(PushedAuthorizationRequest request,
                                                  CancellationToken ct);
}

public sealed class PushedAuthorizationService : IPushedAuthorizationService
{
    private const int ParLifetimeSeconds = 600;

    private readonly IPushedAuthorizationValidator _pushedAuthorizationValidator;
    private readonly IParEntryRepository _parEntryRepository;
    private readonly IAuthorizationSessionRepository _authorizationSessionRepository;
    private readonly IClientRepository _clientRepository;

    public PushedAuthorizationService(
        IPushedAuthorizationValidator pushedAuthorizationValidator,
        IParEntryRepository parEntryRepository,
        IAuthorizationSessionRepository authorizationSessionRepository,
        IClientRepository clientRepository)
    {
        _pushedAuthorizationValidator = pushedAuthorizationValidator;
        _parEntryRepository = parEntryRepository;
        _authorizationSessionRepository = authorizationSessionRepository;
        _clientRepository = clientRepository;
    }

    public async Task<PushedAuthorizationResponse> HandleAsync(PushedAuthorizationRequest request,
                                                               CancellationToken ct)
    {
        if (!string.Equals(request.CodeChallengeMethod, "S256", StringComparison.Ordinal))
            throw OAuthException.FromInvalidRequest();

        var client = await _clientRepository.GetByIdAsync(request.ClientId, ct);

        if (client is null)
            throw OAuthException.FromInvalidClient();

        _pushedAuthorizationValidator.Validate(request, client);

        var normalizedScope = NormalizeScope(request.Scope);
        
        var code = SecureCodeGenerator.GenerateBase64UrlToken(length: 32);
        var requestUri = BuildRequestUri(code);

        var now = DateTimeOffset.UtcNow;
        var expiresAt = now.AddSeconds(ParLifetimeSeconds);

        var entry = new ParEntry
        {
            RequestUriCode = code,
            ClientId = client.ClientId,
            RedirectUri = request.RedirectUri,
            Scope = normalizedScope,
            ResponseType = request.ResponseType,
            CodeChallenge = request.CodeChallenge,
            CodeChallengeMethod = request.CodeChallengeMethod,
            State = request.State,
            CreatedAt = now,
            ExpiresAt = expiresAt
        };

        await _parEntryRepository.StoreAsync(requestUri, entry, ct);

        var session = new AuthorizationSession
        {
            SessionId = SecureCodeGenerator.GenerateBase64UrlToken(length: 32),
            RequestUriCode = code,
            Status = AuthorizationStatus.Initialized,
            CreatedAt = now,
            ExpiresAt = expiresAt
        };

        await _authorizationSessionRepository.StoreAsync(session, ct);

        return new PushedAuthorizationResponse
        {
            RequestUri = requestUri,
            ExpiresIn = ParLifetimeSeconds
        };
    }

    private static string BuildRequestUri(string code)
    {
        return $"urn:ietf:params:oauth:request_uri:{code}";
    }

    private static string NormalizeScope(string scope)
    {
        return string.Join(' ',
            scope.Split(' ', StringSplitOptions.RemoveEmptyEntries)
                 .Distinct(StringComparer.Ordinal)
                 .OrderBy(x => x, StringComparer.Ordinal));
    }
}

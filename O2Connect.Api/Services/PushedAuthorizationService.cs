using O2Connect.Api.DataValidators;
using O2Connect.Api.Models.Store;
using O2Connect.Api.Repositories;
using O2Connect.Dto.Requests;
using O2Connect.Dto.Responses;
using System.Security.Cryptography;

namespace O2Connect.Api.Services;

public interface IPushedAuthorizationService
{
    Task<PushedAuthorizationResponse> HandleAsync(PushedAuthorizationRequest request,
                                                  CancellationToken ct);
}

public sealed class PushedAuthorizationService : IPushedAuthorizationService
{
    private readonly IPushedAuthorizationValidator _pushedAuthorizationValidator;
    private readonly IParEntryRepository _parEntryRepository;
    private readonly IParAuthorizationSessionRepository _parAuthorizationSessionRepository;
    private readonly IClientRepository _clientRepository;

    public PushedAuthorizationService(
        IPushedAuthorizationValidator pushedAuthorizationValidator,
        IParEntryRepository parEntryRepository,
        IParAuthorizationSessionRepository parAuthorizationSessionRepository,
        IClientRepository clientRepository)
    {
        _pushedAuthorizationValidator = pushedAuthorizationValidator;
        _parEntryRepository = parEntryRepository;
        _parAuthorizationSessionRepository = parAuthorizationSessionRepository;
        _clientRepository = clientRepository;
    }

    public async Task<PushedAuthorizationResponse> HandleAsync(PushedAuthorizationRequest request,
                                                               CancellationToken ct)
    {
        var client = await _clientRepository.GetByIdAsync(request.ClientId, ct);

        _pushedAuthorizationValidator.Validate(request, client);

        var requestUri = GenerateRequestUri();
        var now = DateTimeOffset.UtcNow;

        Enum.TryParse<ParState>(request.State, out var entryState);

        var entry = new ParEntry
        {
            RequestUri = requestUri,
            ClientId = request.ClientId,
            RedirectUri = request.RedirectUri,
            Scope = request.Scope,
            ResponseType = request.ResponseType,
            State = entryState,
            CodeChallenge = request.CodeChallenge,
            CodeChallengeMethod = request.CodeChallengeMethod,
            CreatedAt = now,
            ExpiresAt = now.AddSeconds(600)
        };

        await _parEntryRepository.StoreAsync(requestUri, entry, ct);

        var parSession = new ParAuthorizationSession
        {
            ClientId = request.ClientId,
            CodeChallenge = request.CodeChallenge,
            CodeChallengeMethod = request.CodeChallengeMethod,
            CreatedAt = now,
            RedirectUri = request.RedirectUri,
            Scope = request.Scope,
            SessionId = entry.RequestUri,
            State = ParAuthState.Initialized
        };

        await _parAuthorizationSessionRepository.StoreAsync(parSession, ct);

        return new PushedAuthorizationResponse
        {
            RequestUri = requestUri,
            ExpiresIn = 600
        };
    }

    private static string GenerateRequestUri()
    {
        Span<byte> bytes = stackalloc byte[32];
        RandomNumberGenerator.Fill(bytes);

        var code = Convert.ToBase64String(bytes)
                          .TrimEnd('=')
                          .Replace('+', '-')
                          .Replace('/', '_');

        return $"urn:ietf:params:oauth:request_uri:{code}";
    }
}

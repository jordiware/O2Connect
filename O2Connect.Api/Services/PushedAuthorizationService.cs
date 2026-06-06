using O2Connect.Api.DataValidators;
using O2Connect.Api.Exceptions;
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
    private readonly IClientRepository _clientRepository;

    public PushedAuthorizationService(
        IPushedAuthorizationValidator pushedAuthorizationValidator,
        IParEntryRepository parEntryRepository,
        IClientRepository clientRepository)
    {
        _pushedAuthorizationValidator = pushedAuthorizationValidator;
        _parEntryRepository = parEntryRepository;
        _clientRepository = clientRepository;
    }

    public async Task<PushedAuthorizationResponse> HandleAsync(PushedAuthorizationRequest request,
                                                               CancellationToken ct)
    {
        var client = await _clientRepository.GetByIdAsync(request.ClientId, ct);

        _pushedAuthorizationValidator.Validate(request, client);

        var requestUri = GenerateRequestUri();

        var entry = new ParEntry
        {
            ClientId = request.ClientId,
            RedirectUri = request.RedirectUri,
            Scope = request.Scope,
            ResponseType = request.ResponseType,
            State = request.State,
            CodeChallenge = request.CodeChallenge,
            CodeChallengeMethod = request.CodeChallengeMethod,
            CreatedAt = DateTimeOffset.UtcNow,
            ExpiresInSeconds = 600
        };

        await _parEntryRepository.StoreAsync(requestUri, entry, ct);

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

namespace O2Connect.Api.Crypto;

public interface ISecureTokenGenerator
{
    string GenerateSecureToken(int numBytes = 64);
}

public class SecureTokenGenerator : ISecureTokenGenerator
{
    public string GenerateSecureToken(int numBytes = 64)
    {
        return SecureCodeGenerator.GenerateBase64UrlToken(length: numBytes, prefix: "rt_");
    }
}

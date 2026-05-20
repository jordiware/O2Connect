namespace O2Connect.Api.Crypto;

public interface ISecretHasher
{
    string Hash(string secret);
    bool Verify(string secret, string hashedSecret);
}

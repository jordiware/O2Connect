namespace O2Connect.Api.Models;

public enum SigningKeyStatus
{
    Active,   // used for signing new tokens
    Retired,  // used only for validation (JWKS only)
    Expired   // no longer needed for validation
}

namespace Fancy.ResourceLinker.Gateway.Authentication;

/// <summary>
/// A base class for custom token service exceptions.
/// </summary>
internal abstract class TokenServiceException : Exception {}

/// <summary>
/// Custom exception type to indicate that a session id is required but is not available.
/// </summary>
internal class NoSessionIdException : TokenServiceException
{
}

/// <summary>
/// Custom exception type to indicate that there is a valid session but no token is available for this session.
/// </summary>
/// <seealso cref="Fancy.ResourceLinker.Gateway.Authentication.TokenServiceException" />
internal class NoTokenForCurrentSessionIdException : TokenServiceException
{
}

/// <summary>
/// Custom exception type to indicate that something with the token refresh went wrong.
/// </summary>
internal class TokenRefreshException : TokenServiceException
{
}

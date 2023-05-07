namespace Fancy.ResourceLinker.Gateway.Authentication;

/// <summary>
/// A special exception to indicate that something with the token refresh went wrong.
/// </summary>
/// <seealso cref="System.Exception" />
internal class TokenRefreshException : Exception
{
}

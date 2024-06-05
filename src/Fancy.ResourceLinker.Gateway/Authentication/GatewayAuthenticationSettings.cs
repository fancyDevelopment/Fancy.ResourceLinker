namespace Fancy.ResourceLinker.Gateway.Authentication;

/// <summary>
/// A class to hold all settings required to configure the gateway authentication feature.
/// </summary>
public class GatewayAuthenticationSettings
{
    /// <summary>
    /// Gets or sets the session timeout in minutes.
    /// </summary>
    /// <value>
    /// The session timeout in minutes.
    /// </value>
    public int SessionTimeoutInMin { get; set; } = 5;

    /// <summary>
    /// Gets or sets the URL to the authority.
    /// </summary>
    /// <value>
    /// The authority URL.
    /// </value>
    public string Authority { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the client identifier.
    /// </summary>
    /// <value>
    /// The client identifier.
    /// </value>
    public string ClientId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the client secret.
    /// </summary>
    /// <value>
    /// The client secret.
    /// </value>
    public string ClientSecret { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the scopes to request during an authorization code flow.
    /// </summary>
    /// <value>
    /// The authorization code scopes.
    /// </value>
    public string AuthorizationCodeScopes { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the scopes to request during an client credentials flow.
    /// </summary>
    /// <value>
    /// The client credentials scopes.
    /// </value>
    public string ClientCredentialsScopes { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the type of the unique identifier claim.
    /// </summary>
    /// <value>
    /// The type of the unique identifier claim.
    /// </value>
    public string UniqueIdentifierClaimType { get; set; } = string.Empty;

    /// <summary>
    /// Set whether the handler should go to user info endpoint of the authorization server to retrieve additional claims or not 
    /// after creating an identity from id_token received from token endpoint.
    /// </summary>
    /// <value>
    ///   <c>true</c> if the handler shall query the user info endpoint of the authorization server; otherwise, <c>false</c>.
    /// </value>
    public bool QueryUserInfoEndpoint { get; set; } = false;

    /// <summary>
    /// Gets or sets the issuer address for sign out.
    /// </summary>
    /// <value>
    /// The issuer address for sign out.
    /// </value>
    public string? IssuerAddressForSignOut { get; set; }

    /// <summary>
    /// Gets or sets the cookiesettings.
    /// </summary>
    /// <value>
    /// Set cookiesettings.
    /// </value>
    public CookieSettings CookieSettings { get; set; } = new CookieSettings();


    public void Validate()
    {
        // Check required fields
        if(string.IsNullOrEmpty(Authority)) 
        {
            throw new InvalidOperationException("'Authority' is required to be set within 'AuthenticationSettings'");
        }

        if (string.IsNullOrEmpty(ClientId))
        {
            throw new InvalidOperationException("'ClientId' is required to be set within 'AuthenticationSettings'");
        }

        if (string.IsNullOrEmpty(UniqueIdentifierClaimType))
        {
            throw new InvalidOperationException("'UniqueIdentifierClaimType' is required to be set within 'AuthenticationSettings'");
        }
    }
}

public class CookieSettings
{
    /// <summary>
    /// Gets or sets the SameSite policy
    /// </summary>
    /// <value>
    /// true || Not set == Strict
    /// false == Lax
    /// </value>
    public bool SameSiteStrict { get; set; } = true;
    ///                   
    /// <summary>
    /// Gets or sets the secure flag.
    /// </summary>
    /// <value>
    /// true || Not set == Always
    /// false == SameAsRequest
    /// </value>
    public bool Secure { get; set; } = true;
    ///                   
    /// <summary>
    /// Gets or sets the HttpOnly setting.
    /// </summary>
    /// <value>
    /// true || Not set == true
    /// false == false
    /// </value>
    public bool HttpOnly { get; set; } = true;                 
}

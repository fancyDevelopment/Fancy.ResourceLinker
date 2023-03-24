namespace Fancy.ResourceLinker.Gateway.Routing;

public enum AuthenticationType
{
    None, 

    Authorize,

    ApiAuthorize
}

public class RouteSettings
{
    public string PathMatch { get; set; }
    public Uri BaseUrl { get; set; }
    public bool EnforceAuthentication { get; set; }
    public AuthenticationType Authentication { get; set; }
}

public class GatewayRoutingSettings
{
    public IDictionary<string, RouteSettings> Routes { get; set; }
}

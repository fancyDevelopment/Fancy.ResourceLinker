namespace Fancy.ResourceLinker.Gateway.Routing.Auth;

/// <summary>
/// A class to create instances of auth strategies for specific routes.
/// </summary>
public class AuthStrategyFactory
{
    /// <summary>
    /// The authentication strategies.
    /// </summary>
    private Dictionary<string, Type> _authStrategies = new Dictionary<string, Type>();

    /// <summary>
    /// The authentication strategy instances.
    /// </summary>
    private Dictionary<string, IAuthStrategy> _authStrategyInstances = new Dictionary<string, IAuthStrategy>();

    /// <summary>
    /// The routing settings.
    /// </summary>
    private readonly GatewayRoutingSettings _routingSettings;

    /// <summary>
    /// Initializes a new instance of the <see cref="AuthStrategyFactory" /> class.
    /// </summary>
    /// <param name="routingSettings">The routing settings.</param>
    public AuthStrategyFactory(GatewayRoutingSettings routingSettings)
    {
        _routingSettings = routingSettings;
    }

    /// <summary>
    /// Gets the authentication strategy.
    /// </summary>
    /// <param name="name">The name.</param>
    /// <returns>An instance of the auth strategy.</returns>
    public IAuthStrategy GetAuthStrategy(string route)
    {
        if(! _authStrategyInstances.ContainsKey(route))
        {
            string strategyName = _routingSettings.Routes[route].Authentication.Strategy;

            Type authStrategyType = _authStrategies[strategyName];

            // Create a new instance of the auth strategy
            IAuthStrategy? instance = Activator.CreateInstance(authStrategyType, _routingSettings.Routes[route].Authentication) as IAuthStrategy;

            if (instance == null) throw new InvalidOperationException("Auth strategy could not be instantiated");

            _authStrategyInstances[route] = instance;
        }

        return _authStrategyInstances[route];
    }

    public void AddAuthStrategy(string name, Type authStrategyType)
    {
        _authStrategies.Add(name, authStrategyType);
    }

    /// <summary>
    /// Clears the authentication strategies.
    /// </summary>
    public void ClearAuthStrategies()
    {
        _authStrategies.Clear();
    }
}

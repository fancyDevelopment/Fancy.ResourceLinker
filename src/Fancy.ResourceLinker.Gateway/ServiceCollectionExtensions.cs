using Fancy.ResourceLinker.Gateway.AntiForgery;
using Fancy.ResourceLinker.Gateway.Authentication;
using Fancy.ResourceLinker.Gateway.Common;
using Fancy.ResourceLinker.Gateway.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Fancy.ResourceLinker.Gateway;

/// <summary>
/// A class with the required context to build a gateway configuration.
/// </summary>
public class GatewayBuilder
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GatewayBuilder"/> class.
    /// </summary>
    /// <param name="services">The services.</param>
    internal GatewayBuilder(IServiceCollection services)
    {
        Services = services;
    }

    /// <summary>
    /// Gets the services.
    /// </summary>
    /// <value>
    /// The services.
    /// </value>
    public IServiceCollection Services { get; }
}

/// <summary>
/// A class with the a configured context to build a gateway.
/// </summary>
public class ConfiguredGatewayBuilder : GatewayBuilder
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ConfiguredGatewayBuilder"/> class.
    /// </summary>
    /// <param name="services">The services.</param>
    /// <param name="settings">The settings.</param>
    internal ConfiguredGatewayBuilder(IServiceCollection services, GatewaySettings settings) : base(services)
    {
        Settings = settings;
    }

    /// <summary>
    /// Gets the settings.
    /// </summary>
    /// <value>
    /// The settings.
    /// </value>
    public GatewaySettings Settings { get; }
}

/// <summary>
/// A class with the required context to build a antiforgery configuration.
/// </summary>
public class GatewayAntiForgeryBuilder : GatewayBuilder
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GatewayAntiForgeryBuilder"/> class.
    /// </summary>
    /// <param name="services">The services.</param>
    public GatewayAntiForgeryBuilder(IServiceCollection services) : base(services) { }
}

/// <summary>
/// A class with the required context to build a authentication configuration.
/// </summary>
public class GatewayAuthenticationBuilder : GatewayBuilder
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GatewayAuthenticationBuilder"/> class.
    /// </summary>
    /// <param name="services">The services.</param>
    public GatewayAuthenticationBuilder(IServiceCollection services) : base(services) { }
}

/// <summary>
/// A class with the required context to build a routing configuration.
/// </summary>
public class GatewayRoutingBuilder : GatewayBuilder
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GatewayRoutingBuilder"/> class.
    /// </summary>
    /// <param name="services">The services.</param>
    public GatewayRoutingBuilder(IServiceCollection services) : base(services) { }
}

/// <summary>
/// Extension class with helpers to easily register the gateway to ioc container.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds the gateway to the ioc container.
    /// </summary>
    /// <param name="services">The services.</param>
    /// <returns>A gateway builder.</returns>
    public static GatewayBuilder AddGateway(this IServiceCollection services)
    {
        GatewayCommon.AddGatewayCommonServices(services);
        return new GatewayBuilder(services);
    }

    /// <summary>
    /// Loads the configuration for the gateway.
    /// </summary>
    /// <param name="gatewayBuilder">The gateway builder.</param>
    /// <param name="config">The configuration.</param>
    /// <returns>A configured gateway builder.</returns>
    public static ConfiguredGatewayBuilder LoadConfiguration(this GatewayBuilder gatewayBuilder, IConfiguration config)
    {
        GatewaySettings? settings = config.Get<GatewaySettings>();

        if(settings == null)
        {
            throw new InvalidOperationException("The provided configuration does not contain proper settings");
        }

        if (settings.Authentication != null) gatewayBuilder.Services.AddSingleton(settings.Authentication);
        if (settings.Routing != null) gatewayBuilder.Services.AddSingleton(settings.Routing);
        ConfiguredGatewayBuilder configuredGatewayBuilder = new ConfiguredGatewayBuilder(gatewayBuilder.Services, settings);
        return configuredGatewayBuilder;
    }

    /// <summary>
    /// Adds the anti forgery feature to the gateway.
    /// </summary>
    /// <typeparam name="T">A subclass of the gateway builder.</typeparam>
    /// <param name="gatewayBuilder">The gateway builder.</param>
    /// <returns>A subclass of the gateway builder.</returns>
    public static T AddAntiForgery<T>(this T gatewayBuilder) where T : GatewayBuilder
    {
        GatewayAntiForgery.AddGatewayAntiForgery(gatewayBuilder.Services);
        return gatewayBuilder;
    }

    /// <summary>
    /// Adds the anti forgery.
    /// </summary>
    /// <typeparam name="T">A subclass of the gateway builder.</typeparam>
    /// <param name="gatewayBuilder">The gateway builder.</param>
    /// <param name="buildOptions">The build options.</param>
    /// <returns>A subclass of the gateway builder.</returns>
    public static T AddAntiForgery<T>(this T gatewayBuilder, Action<GatewayAntiForgeryBuilder> buildOptions) where T : GatewayBuilder
    {
        GatewayAntiForgery.AddGatewayAntiForgery(gatewayBuilder.Services);
        buildOptions(new GatewayAntiForgeryBuilder(gatewayBuilder.Services));
        return gatewayBuilder;
    }

    /// <summary>
    /// Adds the authentication feature to the gateway with the default in memory token store.
    /// </summary>
    /// <param name="configuredGatewayBuilder">The configured gateway builder.</param>
    /// <returns>A configured gateway builder.</returns>
    public static ConfiguredGatewayBuilder AddAuthentication(this ConfiguredGatewayBuilder configuredGatewayBuilder)
    {
        AddAuthentication(configuredGatewayBuilder, options => options.UseInMemoryTokenStore());
        return configuredGatewayBuilder;
    }

    /// <summary>
    /// Adds the authentication feature to the gateway.
    /// </summary>
    /// <param name="configuredGatewayBuilder">The configured gateway builder.</param>
    /// <param name="buildOptions">The build options.</param>
    /// <returns>A configured gateway builder.</returns>
    public static ConfiguredGatewayBuilder AddAuthentication(this ConfiguredGatewayBuilder configuredGatewayBuilder,
                                                             Action<GatewayAuthenticationBuilder> buildOptions)
    {
        if(configuredGatewayBuilder.Settings.Authentication == null)
        {
            throw new InvalidOperationException("You can add authentication only if you have provided settings for it");
        }

        configuredGatewayBuilder.Settings.Authentication.Validate();

        GatewayAuthentication.AddGatewayAuthentication(configuredGatewayBuilder.Services, configuredGatewayBuilder.Settings.Authentication);
        buildOptions(new GatewayAuthenticationBuilder(configuredGatewayBuilder.Services));
        return configuredGatewayBuilder;
    }

    /// <summary>
    /// Adds the routing feature to the gateway with the default in memory configuration.
    /// </summary>
    /// <param name="configuredGatewayBuilder">The configured gateway builder.</param>
    /// <returns>A configured gateway builder.</returns>
    public static ConfiguredGatewayBuilder AddRouting(this ConfiguredGatewayBuilder configuredGatewayBuilder)
    {
        AddRouting(configuredGatewayBuilder, options => options.UseInMemoryResourceCache());
        return configuredGatewayBuilder;
    }

    /// <summary>
    /// Adds the routing feature to the gateway.
    /// </summary>
    /// <param name="configuredGatewayBuilder">The configured gateway builder.</param>
    /// <param name="buildOptions">The build options.</param>
    /// <returns>A configured gateway builder.</returns>
    public static ConfiguredGatewayBuilder AddRouting(this ConfiguredGatewayBuilder configuredGatewayBuilder,
                                                           Action<GatewayRoutingBuilder> buildOptions)
    {
        if (configuredGatewayBuilder.Settings.Routing == null)
        {
            throw new InvalidOperationException("You can add routing only if you have provided settings for it");
        }

        configuredGatewayBuilder.Settings.Routing.Validate();

        GatewayRouting.AddGatewayRouting(configuredGatewayBuilder.Services, configuredGatewayBuilder.Settings.Routing);
        buildOptions(new GatewayRoutingBuilder(configuredGatewayBuilder.Services));
        return configuredGatewayBuilder;
    }

    /// <summary>
    /// Uses the in memory token store for the authentication feature.
    /// </summary>
    /// <param name="gatewayAuthenticationBuilder">The gateway authentication builder.</param>
    public static void UseInMemoryTokenStore(this GatewayAuthenticationBuilder gatewayAuthenticationBuilder)
    {
        gatewayAuthenticationBuilder.Services.AddSingleton<ITokenStore, InMemoryTokenStore>();
    }

    /// <summary>
    /// Uses the in memory resource cache for the routing builder.
    /// </summary>
    /// <param name="gatewayRoutingBuilder">The gateway routing builder.</param>
    public static void UseInMemoryResourceCache(this GatewayRoutingBuilder gatewayRoutingBuilder)
    {
        gatewayRoutingBuilder.Services.AddSingleton<IResourceCache, InMemoryResourceCache>();
    }
}

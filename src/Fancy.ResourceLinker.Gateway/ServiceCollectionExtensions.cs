using Fancy.ResourceLinker.Gateway.AntiForgery;
using Fancy.ResourceLinker.Gateway.Authentication;
using Fancy.ResourceLinker.Gateway.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Fancy.ResourceLinker.Gateway
{
    public class GatewayBuilder
    {
        public GatewayBuilder(IServiceCollection services)
        {
            Services = services;
        }

        public IServiceCollection Services { get; }
    }

    public class ConfiguredGatewayBuilder : GatewayBuilder
    {
        public ConfiguredGatewayBuilder(IServiceCollection services, GatewaySettings settings) : base(services)
        {
            Settings = settings;
        }

        public GatewaySettings Settings { get; }
    }

    public class GatewayAntiForgeryBuilder : GatewayBuilder
    {
        public GatewayAntiForgeryBuilder(IServiceCollection services) : base(services)
        {
        }
    }

    public class GatewayAuthenticationBuilder : GatewayBuilder
    {
        public GatewayAuthenticationBuilder(IServiceCollection services) : base(services)
        {
        }
    }

    public class GatewayRoutingBuilder : GatewayBuilder
    {
        public GatewayRoutingBuilder(IServiceCollection services) : base(services)
        {
        }
    }

    public static class ServiceCollectionExtensions
    {
        public static GatewayBuilder AddGateway(this IServiceCollection services)
        {
            return new GatewayBuilder(services);
        }

        public static ConfiguredGatewayBuilder LoadConfiguration(this GatewayBuilder gatewayBuilder, IConfiguration config)
        {
            GatewaySettings settings = config.Get<GatewaySettings>();
            if (settings.Authentication != null) gatewayBuilder.Services.AddSingleton(settings.Authentication);
            if (settings.Routing != null) gatewayBuilder.Services.AddSingleton(settings.Routing);
            ConfiguredGatewayBuilder configuredGatewayBuilder = new ConfiguredGatewayBuilder(gatewayBuilder.Services, settings);
            return configuredGatewayBuilder;
        }

        public static T AddAntiForgery<T>(this T gatewayBuilder) where T : GatewayBuilder
        {
            GatewayAntiForgery.AddGatewayAntiForgery(gatewayBuilder.Services);
            return gatewayBuilder;
        }

        public static T AddAntiForgery<T>(this T gatewayBuilder, Action<GatewayAntiForgeryBuilder> buildOptions) where T : GatewayBuilder
        {
            GatewayAntiForgery.AddGatewayAntiForgery(gatewayBuilder.Services);
            buildOptions(new GatewayAntiForgeryBuilder(gatewayBuilder.Services));
            return gatewayBuilder;
        }

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

        public static void UseInMemoryTokenStore(this GatewayAuthenticationBuilder builder)
        {
            builder.Services.AddSingleton<ITokenStore, InMemoryTokenStore>();
        }

        public static void UseInMemoryResourceCache(this GatewayRoutingBuilder builder)
        {
            builder.Services.AddSingleton<IResourceCache, InMemoryResourceCache>();
        }
    }
}

using System;
using System.Collections.Generic;

namespace Fancy.ResourceLinker.Gateway.Routing
{
    public class MicroserviceSettings
    {
        public Uri BaseUrl { get; set; }
    }

    public class GatewayRoutingSettings
    {
        public IDictionary<string, MicroserviceSettings> Microservices { get; set; }
    }
}

using Fancy.ResourceLinker.Gateway.Authentication;
using Fancy.ResourceLinker.Gateway.Routing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fancy.ResourceLinker.Gateway
{
    public class GatewaySettings
    {
        public GatewayAuthenticationSettings? Authentication { get; set; }

        public GatewayRoutingSettings? Routing { get; set; }
    }
}

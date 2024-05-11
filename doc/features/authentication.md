# Authentication

The Gateway supports two modes of authentication. One is that you can hide a SPA (Single Page Application) behind the gateway and trigger and run the OAuth Code Flow + PKCE from the server side. The other is that the gateway is able to authenticate itself at the resource servers it calls. 

## Authentication at the Gateway for a Single Page Application

To enable the authentication based on OAuth Code Flow + PKCE add the Authentication feature when registering the library to the services as shown in the snippet below:

```cs
builder.Services.AddGateway()
                .LoadConfiguration(builder.Configuration.GetSection("Gateway"))
                .AddRouting()
                .AddAuthentication(); // <-- Add this to activate the authentication feature
```

Extend your `Gateway` configuration section with an `Authentication` section and a route to your frontend.

```json
"Gateway": {
    "Authentication": {  // <-- Add this section and configure it with your values
        "Authority": "<Your Authority URL>",
        "ClientId": "<Your Client ID>",
        "AuthorizationCodeScopes": "<Your Scopes to Request>",
        "SessionTimeoutInMin": 30,
        "UniqueIdentifierClaimType": "<A Claim in the Token which Uniquely Identifes the User>"
    }
    "Routing": {
        "Routes": {
            "Microservice1": {
                "BaseUrl": "http://localhost:5000",
            },
            "Frontend": {   // <-- Add a route to your frontend with a 'PathMatch' to activate the reverse proxy for this route
                "BaseUrl": "http://localhost:4200",
                "PathMatch": "{**path}",
            }
        }
    }
}
```

If you have completed the steps before you can trigger the OAuth flow by loading the frontend from the origin of the gateway and setting the window location in your browser with the help of JavaScript to the `./login` endpoint as shown in the following snippet:

```js
window.location.href  = './login?redirectUri=' + encodeURIComponent(window.origin);
```

You can provide an optional redirect url so that after successfull login you geet automatically redirected to the url you started the login workflow from.

To trigger the logout flow set the window location to the `./logout` endpoint as shown in the following snippent. 

```js
window.location.href  = './logout';
```

To get the ID token into the frontend, the frontend can simply call the `./userinfo` enpoint with a standard http GET request.

## Authentication at the Resource Servers

The gateway provides different authentication strategies you can use to make authenticated calls to your resource servers.

### Token Pass Through

The simplest way to provide a token to a resource server is to just pass through the token you got during authenticating the user at the frontend (e.g. your Single Page Application). In this case the resource server must accept the very same token the gateway gets. To pass through the token configure the authentication at the route with the `TokenPassThrough` auth strategy as shown in the following snippet.

```json
"Gateway": {
    "Authentication": {
        [...]
    },
    "Routing": {
        "Routes": {
            "Microservice1": {
                "BaseUrl": "http://localhost:5000",
                "Authentication": {
                    "Strategy":  "TokenPassThrough"   // <-- Configure the TokenPassThrough auth strategy
                }
            },
            [...]
        }
    }
}
```

### Token Exchange

Typically a resource server requires a token especially for its own audience. In that case the token we got during authenticating the user at the frontend needs to be exchanged to a token for the audience of the resource server. To achive this configure one of the available token exchange auth strategies. 

#### Azure on Behalf of

The Azure on Behalf of flow is proprietary flow of Microsoft to exchange a token for a specific "AppRegistration" within Microsofts EntraID. To enable this flow you need at least two AppRegistrations. One for the Gateway/Frontend and one for each resource server. The resoruce server AppRegistration then has to provide an API and the gateways AppRegistration needs be allowed to use this api. Finally can request a token for the gateway and exchange it to a token for the resource server with the help of the following configuration:

```json
"Gateway": {
    "Authentication": { // <-- Configure the properties of the AppRegistration for the gateway here
        "Authority": "<Your Authority URL>",
        "ClientId": "<Your Client ID>",
        "ClientSecret": "<Your Client Secret>",   // <-- Important, you need a client secret here to make the token exchange work
        "AuthorizationCodeScopes": "<Your Scopes to Request>",
        "SessionTimeoutInMin": 30,
        "UniqueIdentifierClaimType": "<A Claim in the Token which Uniquely Identifes the User>"
    },
    "Routing": {
        "Routes": {
            "Microservice1": {
                "BaseUrl": "http://localhost:5000",
                "Authentication": {
                    "Strategy": "AzureOnBehalfOf", // <-- Set the azure on behalf of auth strategy
                    "Options": {
                        "Scope": "api://microservice1/all" // <-- Set the scope of the api you want to request
                    }
                }
            },
            [...]
        }
    }
}
```

### Client Credentials

If you have an anonymous access to the api of the gateway but the gateway must authenticate to the resource servers you can use one of the supported client credential auth strategies. In that case no `Authentication` configuration is needed and also the `AddAuthentication()` feature dos not need to be added to the service collection. 

#### Standard Client Credentials

In the standard client credential auth strategy we provide the gateway with the necessary data to be able to get a token with the standard OAuth Client Credential flow. To achive this, set up the authentication configuration of a route as follows: 

```json
"Gateway": {
    "Routing": {
        "Routes": {
            "Microservice1": {
                "BaseUrl": "http://localhost:5000",
                "Authentication": {
                    "Strategy": "ClientCredentialOnly",
                    "Options": {
                        "Authority": "<Your Authority URL>",
                        "ClientId": "<Your Client ID>",
                        "ClientSecret": "<Your Client Secret>",
                        "Scope": "<Scopes to Request>",
                    }
                }
            },
            [...]
        }
    }
}
```

#### Auth0 Client Credentials

In case you would like to use the client credential auth strategy with Auth0 as a token server, you typically have to provide an additional audience parameter in the token request. For this a special auth strategy for Auth0 was created. Configure the client credential flow for Auth0 as follows:

```json
"Gateway": {
    "Routing": {
        "Routes": {
            "Microservice1": {
                "BaseUrl": "http://localhost:5000",
                "Authentication": {
                    "Strategy": "Auth0ClientCredentialOnly",
                    "Options": {
                        "Authority": "<Your Authority URL>",
                        "ClientId": "<Your Client ID>",
                        "ClientSecret": "<Your Client Secret>",
                        "Scope": "<Scopes to Request>",
                        "Audience": "<Audience of the API to Request>" // <-- This additional parameter is needed by Auth0
                    }
                }
            },
            [...]
        }
    }
}
```

### Custom Auth Strategy

ToDo!
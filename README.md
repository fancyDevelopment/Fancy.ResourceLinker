# Fancy.ResourceLinker
A library to easily create API Gateways and Backend for Frontends (BFF) based on ASP.NET Core.

With Fancy Resource Linker you can easily add the following features to your API Gateway: 

- **Data Aggregation** 

  Aggregate data from different sources (e.g. different Microservices) into view models which are optimized for your client. Reduce the calls of the client to the backend by filling an entire view in your client with a single request and thus boosting your client side user experience.

- **Reverse Proxy**
  
  Publish separate deployed Apps or Microfrontends or Microservices under one orign within different virtual directories.

- **Truly RESTful APIs utilizing HATEOAS**

  Create web api's which enrich your json documents with metadata about connected/linked resources and with possible actions which can be executed on your data to create a full self describing api by the use of hypermedia.

- **Authentication Gateway**
  
  Let your gateway act as a single authentication facade to all of your resources behind, running OAuth flows server side and keeping tokens also only server side to obtain the maximum security and comply to all current security standards.


## Getting Started

First add the [Fancy.ResourceLinker.Gateway](https://www.nuget.org/packages/Fancy.ResourceLinker.Gateway) nuget package to your project.

To get started building an api gateway with Fancy.ResourceLinker in your ASP.NET Core application add the required services to the service collection and load a configuration by providing a configuration section.

```cs
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddGateway()
                .LoadConfiguration(builder.Configuration.GetSection("Gateway"));
```

In your application add a configuration section with the name `"Gateway"` and create the default structure within it as shown in the following snippet:

```json
"Gateway": {
  "Routing": {
    "Routes": {
  }
}
```

With those three steps the library does not do anything yet but is prepared to realize one or more of the features mentioned at the beginning. 

## Realize Features in your Gateway

To learn how each single feature can be realized have a look to the following individual guidelines.

* Aggregating data from differend sources into a client optimized model - Comming Soon
* Provide different apis and/or resources under the same origin - Comming Soon
* Create truly RESTful services - Comming Soon
* Let the gateway act as authentication facade - Comming Soon



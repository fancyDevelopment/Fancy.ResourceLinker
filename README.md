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

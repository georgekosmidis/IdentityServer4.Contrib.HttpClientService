# An HttpClient service for IdentityServer4

[![Build Status](https://dev.azure.com/georgekosmidis/IdentityServer4.Contrib.HttpClientService/_apis/build/status/georgekosmidis.IdentityServer4.Contrib.HttpClientService?branchName=master)](https://dev.azure.com/georgekosmidis/IdentityServer4.Contrib.HttpClientService/_build/latest?definitionId=5&branchName=master) ![Nuget](https://img.shields.io/nuget/v/IdentityServer4.Contrib.HttpClientService)

An HttpClient service that makes it easy to make authenticated HTTP requests to protected by IdentityServer4 resources. Complex types are automatically serialized for requests /  deserialized form responses, all with a fluent interface design:

```csharp
var responseObject = await _requestServiceFactory
                          //Create a instance of the service
                          .CreateHttpClientService()
                          //Also supports IOptions<>
                          .SetIdentityServerOptions("appsettings_section")
                          //GET and deserialize the response body to IEnumerable<Customers>
                          .GetAsync<IEnumerable<Customers>>("https://api/customers");
```

The variable **[responseObject](https://georgekosmidis.github.io/IdentityServer4.Contrib.HttpClientService/api/IdentityServer4.Contrib.HttpClientService.Models.ResponseObject-1.html)** contains multiple properties: from the entire `HttpResponseMessage` and `HttpRequestMessage`, to `HttpStatusCode` and `HttpResponseHeaders`. The most *exciting* feature though, is the `TResponseBody BodyAsType` property which will contain deserializabled complex types from JSON responses (in the example above the `BodyAsType` will be of type `IEnumerable<Customers>`). Check the [ResponseObject&lt;TResponseBody&gt;](https://georgekosmidis.github.io/IdentityServer4.Contrib.HttpClientService/api/IdentityServer4.Contrib.HttpClientService.Models.ResponseObject-1.html) in the docs.

Setting up complex request body for POST, PUT and PATCH requests is also very easy! Here are some examples:

```csharp
var responseObject = await _requestServiceFactory
                          //Create a instance of the service
                          .CreateHttpClientService()
                          //.PostAsync<TRequestBody,TResponseBody>(URL, customer of type Customer1)
                          .PostAsync<Customer1,Customer2>("https://api/customers", customer);
```

You can also fine tune encoding and media-type by using the `TypeContent(TRequestBody model, Encoding encoding, string mediaType)` like this:

```csharp
var responseObject = await _requestServiceFactory
                          //Create a instance of the service
                          .CreateHttpClientService()
                          //.PostAsync<TRequestBody,TResponseBody>(URL, customer of type Customer1)
                          .PostAsync<TypeContent<Customer1>,Customer2>("https://api/customers", new TypeContent(customer, Encoding.UTF8, "application/json"));
```

HTTP verbs supported are: [GET](https://georgekosmidis.github.io/IdentityServer4.Contrib.HttpClientService/api/IdentityServer4.Contrib.HttpClientService.Extensions.HttpClientServiceGetExtensions.html), [POST](https://georgekosmidis.github.io/IdentityServer4.Contrib.HttpClientService/api/IdentityServer4.Contrib.HttpClientService.Extensions.HttpClientServicePostExtensions.html), [PUT](https://georgekosmidis.github.io/IdentityServer4.Contrib.HttpClientService/api/IdentityServer4.Contrib.HttpClientService.Extensions.HttpClientServicePutExtensions.html), [DELETE](https://georgekosmidis.github.io/IdentityServer4.Contrib.HttpClientService/api/IdentityServer4.Contrib.HttpClientService.Extensions.HttpClientServiceDeleteExtensions.html), [PATCH](https://georgekosmidis.github.io/IdentityServer4.Contrib.HttpClientService/api/IdentityServer4.Contrib.HttpClientService.Extensions.HttpClientServicePatchExtensions.html) and [HEAD](https://georgekosmidis.github.io/IdentityServer4.Contrib.HttpClientService/api/IdentityServer4.Contrib.HttpClientService.Extensions.HttpClientServiceHeadExtensions.html). Read the [Documentation](api/index.md) for technical details, check the [features sample](https://github.com/georgekosmidis/IdentityServer4.Contrib.HttpClientService/tree/master/samples/IdentityServer4.Contrib.HttpClientService.FeaturesSample) or a more [complete one](https://github.com/georgekosmidis/IdentityServer4.Contrib.HttpClientService/tree/master/samples/IdentityServer4.Contrib.HttpClientService.CompleteSample).

## Getting started

Getting started with IdentityServer4.Contrib.HttpClientService is rather easy, you only need three things:

 1. Install the nuget package [IdentityServer4.Contrib.HttpClientService](https://www.nuget.org/packages/IdentityServer4.Contrib.HttpClientService)
 2. Provide the options to authenticate in `appsettings.json`
 3. Register the service in `Startup.cs`

### It's a nuget package!

Install the [IdentityServer4.Contrib.HttpClientService](https://www.nuget.org/packages/IdentityServer4.Contrib.HttpClientService) nuget package, using your favorite way.

### IdentityServer4 client credentials options

Add the IdentityServer4 client credentials options to your appsettings.json

```json
"ProtectedResourceClientCredentialsOptions": {
    "Address": "https://demo.identityserver.io/connect/token",
    "ClientId": "m2m",
    "ClientSecret": "secret",
    "Scopes": "api"
  }
```

*The values above are part of the demo offered in <https://demo.identityserver.io/>*

### Register the service

Register the service In `StartUp.cs` in `ConfigureServices(IServiceCollection services)`:

```csharp
services.AddHttpClientService();
```

If you want to use the [Options pattern](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/configuration/options), create a `ProtectedResourceClientCredentialsOptions` class with the client credential options or inherits from `DefaultClientCredentialOptions`:

```csharp
public class ProtectedResourceClientCredentialsOptions : DefaultClientCredentialOptions
{
}
```

and then use `.Configure<TOptions>(...)`:

```csharp
services.AddHttpClientService()
        .Configure<ProtectedResourceClientCredentialsOptions>(Configuration.GetSection(nameof(ProtectedResourceClientCredentialsOptions)));
```

### You are done!

Inject the `IHttpClientServiceFactory` wherever you want to make the an authenticated requests:

```csharp
public class ProtectedResourceService {

  private readonly IHttpClientServiceFactory _requestServiceFactory;
  
  public ProtectedResourceService(IHttpClientServiceFactory requestServiceFactory)
  {
    _requestServiceFactory = requestServiceFactory;
  }  
  
  public async Task<IEnumerable<Customer>> GetCustomers(){
    var response = await _requestServiceFactory
      .CreateHttpClientService()
      .SetIdentityServerOptions("ProtectedResourceClientCredentialsOptions")
      .GetAsync<IEnumerable<Customer>>("https://protected_resource_that_returns_customers_in_json");
  }
}
```

Or if you used the [Options pattern](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/configuration/options) approach

```csharp
public class ProtectedResourceService {

  private readonly IHttpClientServiceFactory _requestServiceFactory;
  private readonly IOptions<ProtectedResourceClientCredentialsOptions> _identityServerOptions;
  
  public ProtectedResourceService(IHttpClientServiceFactory requestServiceFactory, IOptions<ProtectedResourceClientCredentialsOptions> identityServerOptions)
  {
    _requestServiceFactory = requestServiceFactory;
    _identityServerOptions = identityServerOptions;
  }  
  public async Task<IEnumerable<Customer>> GetCustomers(){
    var response = await _requestServiceFactory
      .CreateHttpClientService()
      .SetIdentityServerOptions(_identityServerOptions)
      .GetAsync<IEnumerable<Customer>>("https://protected_resource_that_returns_customers_in_json");
  }
}
```

## Contributing

Feedback and contibution is more than welcome, as there are many more things to do!

Just as a sample:

1. Expand the [IdentityServer4.Contrib.HttpClientService.CompleteSample](https://github.com/georgekosmidis/IdentityServer4.Contrib.HttpClientService/tree/master/samples/IdentityServer4.Contrib.HttpClientService.CompleteSample) with more functionality.
2. Support `JsonSerializerSettings` for `JsonConvert.DeserializeObject<TResponseBody>(apiResponse.BodyAsString)` in [HttpClientService]( https://github.com/georgekosmidis/IdentityServer4.Contrib.HttpClientService/blob/86262f016173bafd2c9ec4fbe70ac9eb1406042a/src/IdentityServer4.Contrib.HttpClientService/HttpClientService.cs#L300).
3. Support more than `ClientCredentials`.
4. Add logging.

Many more are coming soon and all of them should be [issues](https://github.com/georgekosmidis/IdentityServer4.Contrib.HttpClientService/issues), so feel free to open one and let's start discussing solutions!

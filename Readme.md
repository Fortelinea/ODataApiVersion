# ASP.NET Core OData (8.x) API Versioning Sample

---
This is the project repository for post at: https://devblogs.microsoft.com/odata/api-versioning-extension-with-asp-net-core-odata-8/

For details, please refer to the post.

## Update 3/24/2022

Added OData7Swagger project with .NET 3.1, OData 7.5.14 and Swashbucklet 6.30 to demonstrate controllers with OData $ parameters generated in Swagger docs.

Fixed issues with ODataApiVerion project:
### 1. and 2. Swagger/OData v1 and v2 show both controllers for each API version.

Sam Xu and Hassan Habib helped us to add `[ODataRouteComponent("api/vX")]` to the controllers to fix the duplicate route issues.

```
    [ApiVersion("1")]
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ODataRouteComponent("api/v1")]
    public class CustomersController : ODataController {...}

    [ApiVersion("2")]
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ODataRouteComponent("api/v2")]
    public class CustomersController : ODataController {...}
```


### 3. OData $ parameters missing from Swagger docs 

This functionality is provided by Microsoft.AspNetCore.OData.Versioning.ApiExplorer 5.0.0, which doesn't support OData 8.x.
A workaround was found using https://stackoverflow.com/a/49261778 to manually include the OData parameters for Swagger docs when methods return an `IQuerable<T>`.

---
## Update 3/21/2022

This is copy of https://github.com/xuzhg/MyAspNetCore/tree/master/src/ODataApiVersion

### Upgrades
- Requires VS 2022
- .NET 6
- Microsoft.AspNetCore.OData 8.0.8
- Swashbuckle.AspNetCore 6.3.0

### Objective

This goal of this project is to provide a simple working example showing how to configure OData 8.x with Swagger for a versioned API using URL prefix routing, such as:
- http://localhost:5000/api/v1/Customers
- http://localhost:5000/api/v2/Customers

The API should support multiple versions with:
- seperate controllers for each version (ex. `ODataApiVersion.Controllers.v1.CustomersController` and `ODataApiVersion.Controllers.v2.CustomersController`)
- seperate data models for each version (ex. `ODataApiVersion.Models.v1.Customer` and `ODataApiVersion.Models.v2.Customer`)

Using Swagger to generate the docs, there should be a swagger.json for each version defined (ex. `/swagger/v1/swagger.json` and `/swagger/v2/swagger.json`).

### Issues

1. Swagger v1 and v2 show both controllers for each API version.

![image](Images/api_v1v2_sideBySide.png)

2. `\$odata` debug page shows duplicate endpoint mappings.
v1 controller maps to v1 and v2 endpoints, and v2 controller maps to v1 and v2 templates.

![image](Images/odata_duplicate_endpoints.png)

3. Swagger v1 executes v1 controller (and v2 executes v2); however, `$select`, `$orderby`, etc. query parameters are missing from Swagger doc although they were present with OData 7.x.

![image](Images/api_v1_executesCorrectly.png)

![image](Images/api_v1_odata.png)

### Use Extensions Option

As a thought excersise, I attempted to go back to using the extensions code with updates to look at URL paths for the version. This isn't ideal as the original post descibes OData 8 as having "built-in API versioning functionality via route URL prefix template."

Uncomment the `DefineConstants` lines in ODataApiVersion.csproj to enable the `USE_EXTENSIONS` symbol.

```
<DefineConstants>$(DefineConstants)TRACE;USE_EXTENSIONS</DefineConstants>
```

With this symbol defined, please note the following:
- The controller endpoint URLs are now http://localhost:5000/v1.0/Customers and http://localhost:5000/v2.0/Customers
- Swagger no longer can discover any controllers/actions. This may require additional configuration.
- `ApiController` and `Route` attributes are removed from controllers.
- `EntitySetCustomersSegment` has a new `v{version:apiVersion}` template
- `MyODataRoutingMatcherPolicy` has an additional attempt to read the version from the URL segment

---
## Update at 12/21/2021

Enable OpenAPI/Swagger via customer requirement.

### OpenAPI/Swagger

If you run the sample and send the following request in a Web brower:

`/swagger`, you will get the following (similar) swagger page:

**NOTE:** Executing these queries will result in a 404 error.

![image](Images/api_versioning_swagger.png)


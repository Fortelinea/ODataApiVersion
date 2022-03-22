# ASP.NET Core OData (8.x) API Versioning Sample

---
This is the project repository for post at: https://devblogs.microsoft.com/odata/api-versioning-extension-with-asp-net-core-odata-8/

For details, please refer to the post.

## Update 3/21/2022

This is copy of https://github.com/xuzhg/MyAspNetCore/tree/master/src/ODataApiVersion

### Upgrades
- Requires VS 2022
- .NET 6
- Microsoft.AspNetCore.OData 8.0.8
- Swashbuckle.AspNetCore 6.3.0

### Objective

This goal of this project is to provide a simple working example showing how to configure OData 8.x with Swagger for a versioned API (including multiple versioned controllers and models) using URL prefix routing, such as:
- http://localhost:5000/api/v1/Customers
- http://localhost:5000/api/v2/Customers

### Issues

1. Swagger v1 and v2 show both controllers for each API version.

![image](Images/api_v1v2_sideBySide.png)

2. `\$odata` debug page shows duplicate endpoint mappings.

![image](Images/odata_duplicate_endpoints.png)

3. Swagger v1 executes v1 controller; however, `$select`, `$orderby`, etc. query parameters are missing from Swagger doc although present in OData 7.x.

![image](Images/api_v1_executesCorrectly.png)

![image](Images/api_v1_odata.png)

4. Controllers require additional attributes to show in Swagger docs at all (`ApiController` and `Route`).
```
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class CustomersController : ODataController
```

## Update at 12/21/2021

Enable OpenAPI/Swagger via customer requirement.

### OpenAPI/Swagger

If you run the sample and send the following request in a Web brower:

`/swagger`, you will get the following (similar) swagger page:

**NOTE:** Executing these queries will result in a 404 error.

![image](Images/api_versioning_swagger.png)


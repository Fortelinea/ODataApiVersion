using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNetCore.Mvc;
using OData7Swagger.Models;

namespace OData7Swagger
{
    /// <summary>
    /// Called by IRouteBuilderExtensions.MapVersionedODataRoute in <see cref="Startup"/>.
    /// </summary>
    public class ODataModelConfiguration : IModelConfiguration
    {
        public void Apply(ODataModelBuilder builder, ApiVersion apiVersion, string? routePrefix)
        {
            builder.EntitySet<Customer>("Customers")
                   .EntityType
                   .Select()
                   .Count()
                   .Filter()
                   .OrderBy();
        }
    }
}

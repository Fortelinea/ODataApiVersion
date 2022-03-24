using System.Linq;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace OData7Swagger
{
    internal class RemoveVersionFromParameter : IOperationFilter
    {

        #region IOperationFilter

        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            var versionParameter = operation.Parameters.SingleOrDefault(p => p.Name == "version");
            if (versionParameter != null) operation.Parameters.Remove(versionParameter);
        }

        #endregion

    }

    internal class ReplaceVersionWithExactValueInPath : IDocumentFilter
    {

        #region IDocumentFilter

        public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
        {
            var replacements = swaggerDoc.Paths.ToDictionary(path => path.Key.Replace("v{version}", $"v{swaggerDoc.Info.Version}"), path => path.Value);
            swaggerDoc.Paths = new OpenApiPaths();
            foreach (var (key, value) in replacements) swaggerDoc.Paths.Add(key, value);
        }

        #endregion

    }

    internal class SwaggerDefaultValues : IOperationFilter
    {

        #region IOperationFilter

        /// <summary>
        ///     Applies the filter to the specified operation using the given context.
        /// </summary>
        /// <param name="operation">The operation to apply the filter to.</param>
        /// <param name="context">The current operation filter context.</param>
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            if (operation.Parameters == null)
            {
                return;
            }

            // REF: https://github.com/domaindrivendev/Swashbuckle.AspNetCore/issues/412
            // REF: https://github.com/domaindrivendev/Swashbuckle.AspNetCore/pull/413
            foreach (var parameter in operation.Parameters)
            {
                var description = context.ApiDescription.ParameterDescriptions.First(p => p.Name == parameter.Name);
                var routeInfo = description.RouteInfo;

                if (string.IsNullOrEmpty(parameter.Name))
                {
                    parameter.Name = description.ModelMetadata?.Name;
                }

                if (parameter.Description == null)
                {
                    parameter.Description = description.ModelMetadata?.Description;
                }

                if (routeInfo == null)
                {
                    continue;
                }

                parameter.Required |= !routeInfo.IsOptional;
            }

            // Overwrite description for shared response code
            if (operation.Responses.ContainsKey("400"))
                operation.Responses["400"]
                         .Description = "Invalid query parameter(s). Read the response description";
            if (operation.Responses.ContainsKey("401"))
                operation.Responses["401"]
                         .Description = "Authorization has been denied for this request";
        }

        #endregion

    }
}

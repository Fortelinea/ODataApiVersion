// Copyright saxu@microsoft.com.  All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace ODataApiVersion
{
    public class ConfigureSwaggerOptions : IConfigureOptions<SwaggerGenOptions>
    {
        private readonly IApiVersionDescriptionProvider _provider;

        public ConfigureSwaggerOptions(IApiVersionDescriptionProvider provider)
        {
            _provider = provider;
        }

        #region IConfigureOptions<SwaggerGenOptions>

        public void Configure(SwaggerGenOptions options)
        {
            // Generate swagger.json docs for each version of APIs
            foreach (var description in _provider.ApiVersionDescriptions)
            {
                options.SwaggerDoc(description.GroupName,
                                   new OpenApiInfo
                                   {
                                       Title = $"API v{description.ApiVersion}",
                                       Version = description.ApiVersion.MajorVersion.ToString()
                                   });
            }

            // Add $odata parameters workaround
            options.OperationFilter<ODataParametersSwaggerDefinition>();
        }

        #endregion
    }

    /// <summary>
    /// Help your swagger show OData query options with example pre-fills
    /// </summary>
    /// <remarks>
    /// Adapted from: https://stackoverflow.com/a/49261778
    /// </remarks>
    public class ODataParametersSwaggerDefinition : IOperationFilter
    {
        private static readonly Type QueryableType = typeof(IQueryable);
        private static readonly OpenApiSchema stringSchema = new OpenApiSchema { Type = "string" };
        private static readonly OpenApiSchema intSchema = new OpenApiSchema { Type = "integer" };
        
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            var hasKey = operation.Parameters?.Any(p => string.Equals(p.Name, "key", StringComparison.OrdinalIgnoreCase) ||
                                                        string.Equals(p.Name, "id", StringComparison.OrdinalIgnoreCase));
            var isQueryable = context.MethodInfo.ReturnType.GetInterfaces().Any(i => i == QueryableType);

            if (isQueryable)
            {
                operation.Parameters ??= new List<OpenApiParameter>();


                operation.Parameters.Add(new OpenApiParameter
                                         {
                                             Name = "$select",
                                             Description = "Trim the fields returned using OData syntax",
                                             Example = OpenApiAnyFactory.CreateFor(stringSchema, "Id,ProductName"),
                                             Required = false,
                                             In = ParameterLocation.Query,
                                             Schema = new OpenApiSchema { Type = "string" }
                                         });


                operation.Parameters.Add(new OpenApiParameter
                                         {
                                             Name = "$expand",
                                             Description = "Expand navigation properties returned using OData syntax",
                                             Example = OpenApiAnyFactory.CreateFor(stringSchema, "Id,ProductName"),
                                             Required = false,
                                             In = ParameterLocation.Query,
                                             Schema = new OpenApiSchema { Type = "string" }
                                         });

                // Assumption: If a key is provided, only a single result will be returned.
                if (hasKey == true)
                    return;

                operation.Parameters.Add(new OpenApiParameter
                                         {
                                             Name = "$filter",
                                             Description = "Filter the results using OData syntax.",
                                             Example = OpenApiAnyFactory.CreateFor(stringSchema, "ProductName eq 'YOGURT'"),
                                             Required = false,
                                             In = ParameterLocation.Query,
                                             Schema = stringSchema
                                         });

                operation.Parameters.Add(new OpenApiParameter
                                         {
                                             Name = "$orderby",
                                             Description = "Order the results using OData syntax.",
                                             Example = OpenApiAnyFactory.CreateFor(stringSchema, "Price,ProductName ASC"),
                                             Required = false,
                                             In = ParameterLocation.Query,
                                             Schema = new OpenApiSchema { Type = "string" }
                                         });
                operation.Parameters.Add(new OpenApiParameter
                                         {
                                             Name = "$skip",
                                             Description = "The number of results to skip.",
                                             Example = OpenApiAnyFactory.CreateFor(intSchema, 100),
                                             Required = false,
                                             In = ParameterLocation.Query,
                                             Schema = intSchema
                                         });
                operation.Parameters.Add(new OpenApiParameter
                                         {
                                             Name = "$top",
                                             Description = "The number of results to return.",
                                             Example = OpenApiAnyFactory.CreateFor(intSchema, 50),
                                             Required = false,
                                             In = ParameterLocation.Query,
                                             Schema = intSchema
                                         });
            }
        }
    }

    internal static class OpenApiAnyFactory
    {
        /// <summary>
        /// CreateFor was removed in Swagger 6.x
        /// </summary>
        public static IOpenApiAny CreateFor(OpenApiSchema schema, object value, SchemaRepository schemaRepository = null)
        {
            if (value == null) return null;

            var definition = (schemaRepository != null)
                                 ? ResolveToDefinition(schema, schemaRepository)
                                 : schema;

            if (definition.Type == "integer" && definition.Format == "int64" && TryCast(value, out long longValue))
                return new OpenApiLong(longValue);

            if (definition.Type == "integer" && TryCast(value, out int intValue))
                return new OpenApiInteger(intValue);

            if (definition.Type == "number" && definition.Format == "double" && TryCast(value, out double doubleValue))
                return new OpenApiDouble(doubleValue);

            if (definition.Type == "number" && TryCast(value, out float floatValue))
                return new OpenApiFloat(floatValue);

            if (definition.Type == "boolean" && TryCast(value, out bool boolValue))
                return new OpenApiBoolean(boolValue);

            if (definition.Type == "string" && definition.Format == "date" && TryCast(value, out DateTime dateValue))
                return new OpenApiDate(dateValue);

            if (definition.Type == "string" && definition.Format == "date-time" && TryCast(value, out DateTime dateTimeValue))
                return new OpenApiDate(dateTimeValue);

            if (definition.Type == "string")
                return new OpenApiString(value.ToString());

            return null;
        }
        private static OpenApiSchema ResolveToDefinition(OpenApiSchema schema, SchemaRepository schemaRepository)
        {
            if (schema.AllOf.Any())
                return ResolveToDefinition(schema.AllOf.First(), schemaRepository);

            if (schema.Reference != null && schemaRepository.Schemas.TryGetValue(schema.Reference.Id, out OpenApiSchema referencedSchema))
                return ResolveToDefinition(referencedSchema, schemaRepository);

            return schema;
        }

        private static bool TryCast<T>(object value, out T typedValue)
        {
            try
            {
                typedValue = (T)Convert.ChangeType(value, typeof(T));
                return true;
            }
            catch (InvalidCastException)
            {
                typedValue = default(T);
                return false;
            }
        }
    }
}

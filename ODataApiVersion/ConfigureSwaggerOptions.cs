// Copyright saxu@microsoft.com.  All rights reserved.
// Licensed under the MIT License.

using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace ODataApiVersion
{
    public class ConfigureSwaggerOptions : IConfigureOptions<SwaggerGenOptions>
    {

        private readonly IConfiguration _configuration;

        private readonly IApiVersionDescriptionProvider _provider;

        public ConfigureSwaggerOptions(IApiVersionDescriptionProvider provider, IConfiguration configuration)
        {
            _provider = provider;
            _configuration = configuration;
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
        }

        #endregion
    }
}

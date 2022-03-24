// Copyright saxu@microsoft.com.  All rights reserved.
// Licensed under the MIT License.

using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.AspNetCore.OData;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using ODataApiVersion.Extensions;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace ODataApiVersion
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvcCore().AddApiExplorer();

            services.AddApiVersioning(options =>
                                      {
                                          options.ReportApiVersions = true;
                                          options.AssumeDefaultVersionWhenUnspecified = true;
                                          options.ApiVersionReader = ApiVersionReader.Combine(
                                              new UrlSegmentApiVersionReader()
                                          );
                                      });

            services.AddVersionedApiExplorer(options =>
            {
                options.GroupNameFormat = "'v'V";
                options.SubstituteApiVersionInUrl = true;   // Removes v{version} routes from swagger
            });

            services.AddEndpointsApiExplorer();

            services.AddControllers().AddOData(opt =>
                                               {
                                                   var myODataModelProvider = new MyODataModelProvider();
#if !USE_EXTENSIONS
                                                   opt.AddRouteComponents("api/v1", myODataModelProvider.GetEdmModel("1"))
                                                      .AddRouteComponents("api/v2", myODataModelProvider.GetEdmModel("2"));
#endif

                                                   opt.RouteOptions.EnableKeyInParenthesis = false;
                                                   opt.RouteOptions.EnableNonParenthesisForEmptyParameterFunction = true;
                                                   opt.RouteOptions.EnableQualifiedOperationCall = false;
                                                   opt.RouteOptions.EnableUnqualifiedOperationCall = true;
                                               });

#if USE_EXTENSIONS
            services.TryAddSingleton<IODataModelProvider, MyODataModelProvider>();

            // Adds support for ?api-version=1.0
            services.TryAddEnumerable(
                ServiceDescriptor.Transient<IApplicationModelProvider, MyODataRoutingApplicationModelProvider>());
            services.TryAddEnumerable(ServiceDescriptor.Singleton<MatcherPolicy, MyODataRoutingMatcherPolicy>());
#endif

            services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>();
            services.AddSwaggerGen(
                opt => opt.ResolveConflictingActions(a => a.First()));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IApiVersionDescriptionProvider provider)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            app.UseODataRouteDebug(); // Adds /$odata path to show debug routing table. Remove it if not needed

            app.UseSwagger();
            app.UseSwaggerUI(options =>
                             {
                                 // Add UI dropdown to select all Swagger doc versions
                                 foreach (var description in provider.ApiVersionDescriptions)
                                 {
                                     options.SwaggerEndpoint(
                                         $"/swagger/{description.GroupName}/swagger.json",
                                         description.GroupName.ToUpperInvariant());
                                 }
                             });

        }
    }
}

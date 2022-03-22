// Copyright saxu@microsoft.com.  All rights reserved.
// Licensed under the MIT License.

using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.AspNetCore.OData;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
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
            services.AddApiVersioning(options =>
            {
                options.AssumeDefaultVersionWhenUnspecified = true;
                options.ReportApiVersions = true;
                options.ApiVersionReader = ApiVersionReader.Combine(
                    new UrlSegmentApiVersionReader()
                    // Untested:
                    //,
                    //new QueryStringApiVersionReader("api-version"),
                    //new HeaderApiVersionReader("X-Version"),
                    //new MediaTypeApiVersionReader("ver")
                    );
            });

            services.AddVersionedApiExplorer(setup =>
            {
                setup.GroupNameFormat = "'v'VVV";
                setup.SubstituteApiVersionInUrl = true;
            });

            services.AddEndpointsApiExplorer();

            services.AddControllers().AddOData(opt =>
                                               {
                                                   var myODataModelProvider = new MyODataModelProvider();
                                                   opt.Select()
                                                      .Count()
                                                      .Filter()
                                                      .OrderBy();
#if !USE_EXTENSIONS
                                                   opt.AddRouteComponents("api/v1", myODataModelProvider.GetEdmModel("1.0"))    // Adds v1 controller action to v1 and v2 APIs
                                                      .AddRouteComponents("api/v2", myODataModelProvider.GetEdmModel("2.0"))    // Adds v2 controller actions to v1 and v2 APIs
                                                   ;
#endif

                                                   opt.RouteOptions.EnableKeyInParenthesis = false;
                                                   opt.RouteOptions.EnableNonParenthesisForEmptyParameterFunction = true;
                                                   opt.RouteOptions.EnableQualifiedOperationCall = false;
                                                   opt.RouteOptions.EnableUnqualifiedOperationCall = true;
                                               });

            services.TryAddSingleton<IODataModelProvider, MyODataModelProvider>();

#if USE_EXTENSIONS
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

            app.UseODataRouteDebug(); // Remove it if not needed

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

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}

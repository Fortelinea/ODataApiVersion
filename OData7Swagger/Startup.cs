using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OData7Swagger.Models;

namespace OData7Swagger
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
            services.AddMvcCore();
            services.AddMvc(options => options.EnableEndpointRouting = false)
                    .SetCompatibilityVersion(CompatibilityVersion.Latest);
            
            services.AddRouting();

            services.AddApiVersioning(options =>
                                      {
                                          // reporting api versions will return the headers "api-supported-versions" and "api-deprecated-versions"
                                          options.ReportApiVersions = true;
                                          options.AssumeDefaultVersionWhenUnspecified = true;
                                      });

            services.AddOData()
                    .EnableApiVersioning();

            services.AddVersionedApiExplorer(options => { options.GroupNameFormat = "'v'V"; });

            services.AddODataApiExplorer(options =>
            {
                options.GroupNameFormat = "'v'VVV"; // 'v'major[.minor][-status]
                options.SubstituteApiVersionInUrl = true;
            });

            services.AddSwaggerGen(options =>
                                   {
                                       options.OperationFilter<SwaggerDefaultValues>();
                                       options.OperationFilter<RemoveVersionFromParameter>();

                                       options.DocumentFilter<ReplaceVersionWithExactValueInPath>();
                                       options.EnableAnnotations();
                                       options.CustomSchemaIds(t => t.FullName);
                                   });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, VersionedODataModelBuilder modelBuilder)
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

            app.UseMvc(routeBuilder =>
                       {

                           //routeBuilder.Select()
                           //            .Count()
                           //            .Filter()
                           //            .OrderBy();
                           
                           //var builder = new ODataConventionModelBuilder(app.ApplicationServices);
                           //builder.EntitySet<Customer>("Customers");
                           //routeBuilder.MapODataServiceRoute("ODataRoute", "odata", builder.GetEdmModel());

                           routeBuilder.MapVersionedODataRoute("ODataRoute", "api/v{version:apiVersion}", modelBuilder.GetEdmModels());
                       });

            app.UseSwagger();
            app.UseSwaggerUI(c =>
                             {
                                 c.SwaggerEndpoint("/swagger/v1/swagger.json", "OData 7.5 OpenAPI");
                             });
        }
    }
}

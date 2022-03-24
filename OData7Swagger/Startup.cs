using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

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
            services.AddMvc(options => options.EnableEndpointRouting = false);

            services.AddApiVersioning(options =>
                                      {
                                          // reporting api versions will return the headers "api-supported-versions" and "api-deprecated-versions"
                                          options.ReportApiVersions = true;
                                          options.AssumeDefaultVersionWhenUnspecified = true;
                                      });

            services.AddOData()
                    .EnableApiVersioning();

            services.AddVersionedApiExplorer(options =>
                                             {
                                                 options.GroupNameFormat = "'v'V";
                                             });

            services.AddODataApiExplorer(options =>
            {
                options.GroupNameFormat = "'v'VVV"; // 'v'major[.minor][-status]
                options.SubstituteApiVersionInUrl = true;
            });

            services.AddSwaggerGen();
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

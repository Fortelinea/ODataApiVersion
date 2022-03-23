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
            services.AddMvc(options => options.EnableEndpointRouting = false);

            services.AddOData();

            services.AddSwaggerGen();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
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
                           var builder = new ODataConventionModelBuilder(app.ApplicationServices);
                           builder.EntitySet<Customer>("Customers");

                           routeBuilder.Select()
                                       .Count()
                                       .Filter()
                                       .OrderBy();
                           routeBuilder.MapODataServiceRoute("ODataRoute", "odata", builder.GetEdmModel());
                       });

            app.UseSwagger();
            app.UseSwaggerUI(c =>
                             {
                                 c.SwaggerEndpoint("/swagger/v1/swagger.json", "OData 7.5 OpenAPI");
                             });
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AmadeusHotels.Bll.Config;
using AmadeusHotels.Bll.Services;
using AmadeusHotels.Bll.Services.Interfaces;
using AmadeusHotels.Persistance;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.EntityFrameworkCore;
using AmadeusHotels.Persistance.Repositories.Interfaces;
using AmadeusHotels.Persistance.Repositories;
using AmadeusHotels.Bll.Mapping;
using AmadeusHotels.Persistance.ConfigAppSettings;
using System.Reflection;
using System.IO;
using Microsoft.OpenApi.Models;
using AmadeusHotels.API.Helpers;

namespace AmadeusHotels
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
            services.AddControllers();

            services.Configure<AmadeusClientOptions>(Configuration.GetSection("AmadeusClientOptions"));
            services.Configure<DatabaseOptions>(Configuration.GetSection("DatabaseOptions"));

            //The base extension method registers IHttpClientFactory
            services.AddHttpClient();
            services.AddSingleton<IAmadeusTokenService, AmadeusTokenService>();

            services.AddHttpClient<IAmadeusApiServiceProvider, AmadeusApiServiceProvider>((serviceProvider, cfg) =>
            {
                var amadeusClientOptions = serviceProvider.GetRequiredService<IOptions<AmadeusClientOptions>>();
                cfg.BaseAddress = new Uri(amadeusClientOptions.Value.Url);
            });

            // DB context
            services.AddDbContext<AppDbContext>(options =>
            {
                options.UseSqlServer(Configuration.GetConnectionString("AmadeusHotelsDbLocalContext"), sqlOptions =>
                    sqlOptions.MigrationsHistoryTable(AppDbContext.MigrationsHistoryTableName));
            });

            services.AddAutoMapper(typeof(MappingProfile));

            // Register the Swagger generator, defining 1 or more Swagger documents
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Version = "v1",
                    Title = "AmadeusHotels.API",
                    Description = "Amadeus Hotels API Integration by Josip Divić"

                });

                // Set the comments path for the Swagger JSON and UI.
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                c.IncludeXmlComments(xmlPath);
            });

            services.AddSingleton<MyMemoryCache>();
            services.AddScoped<IHotelsSearchService, HotelsSearchService>();
            services.AddScoped<ISearchRequestRepository, SearchRequestRepository>();
            services.AddScoped<ISearchRequestHotelRepository, SearchRequestHotelRepository>();
            services.AddScoped<IHotelRepository, HotelRepository>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            using (var serviceScope = app.ApplicationServices.GetService<IServiceScopeFactory>().CreateScope())
            {
                var context = serviceScope.ServiceProvider.GetRequiredService<AppDbContext>();
                context.Database.Migrate();
            }

            //app.UseHttpsRedirection();

            // Enable middleware to serve generated Swagger as a JSON endpoint.
            app.UseSwagger();

            // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.),
            // specifying the Swagger JSON endpoint.
            app.UseSwaggerUI(c =>
            {
                //c.SwaggerEndpoint("../swagger/v1/swagger.json", "Amadeus Hotels API Integration");
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Amadeus Hotels API Integration");
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


using EvacuationAPI.Services;
using EvacuationAPI.Services.Helpers;
using Microsoft.OpenApi.Models;

namespace EvacuationAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            // Add to support JSON serialization
            builder.Services.AddControllers()
                .AddNewtonsoftJson();

            // Add Swagger services for API documentation
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c => {
                // Add more groups as needed
                c.SwaggerDoc("Evacuation Zone", new OpenApiInfo { Title = "Evacuation Zone", Version = "v1" });
                c.SwaggerDoc("Vehicle", new OpenApiInfo { Title = "Vehicle", Version = "v1" });
                c.SwaggerDoc("Evacuate", new OpenApiInfo { Title = "Evacuate", Version = "v1" });
                c.SwaggerDoc("Others", new OpenApiInfo { Title = "Others", Version = "v1" });
            });

            // Load configuration from appsettings.json
            var redisConnectionString = builder.Configuration.GetValue<string>("Redis:ConnectionString");

            // Register services
            // Register RedisDb with IConfiguration from the builder
            builder.Services.AddSingleton<RedisDb>();
            builder.Services.AddScoped<EvacuationService>();

            var app = builder.Build();

            // Global error handling middleware
            app.Use(async (context, next) =>
            {
                try
                {
                    await next.Invoke();
                }
                catch (Exception ex)
                {
                    // Log the error (you could use a logging library here like Serilog, NLog, etc.)
                    context.Response.StatusCode = 500;
                    await context.Response.WriteAsync("An error occurred: " + ex.Message);
                }
            });

            // Configure the HTTP request pipeline for development
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    // Swagger UI settings
                    c.SwaggerEndpoint("/swagger/Evacuate/swagger.json", "Evacuate API V1");
                    c.SwaggerEndpoint("/swagger/Evacuation Zone/swagger.json", "Evacuation Zone API V1");
                    c.SwaggerEndpoint("/swagger/Vehicle/swagger.json", "Vehicle API V1");
                    c.SwaggerEndpoint("/swagger/Others/swagger.json", "Others API V1");
                    c.RoutePrefix = "swagger"; // Set the route prefix for Swagger UI
                });
            }

            app.UseHttpsRedirection(); // Redirect HTTP requests to HTTPS

            app.UseAuthorization(); // Apply authorization middleware (if any)

            app.MapControllers(); // Map controller routes

            app.Run(); // Run the application
        }
    }
}

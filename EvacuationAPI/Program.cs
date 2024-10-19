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

            // Add Application Insights telemetry
            builder.Services.AddApplicationInsightsTelemetry(builder.Configuration["ApplicationInsights:InstrumentationKey"]);

            // Add services to the container.
            builder.Services.AddControllers()
                .AddNewtonsoftJson();

            // Add Swagger services for API documentation
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Evacuation API", Version = "v1" });
            });

            // Enable Detailed Errors
            builder.Logging.ClearProviders();
            builder.Logging.AddConsole();

            // Register services
            builder.Services.AddSingleton<RedisDb>();
            builder.Services.AddScoped<EvacuationService>();

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAll",
                    builder =>
                    {
                        builder.AllowAnyOrigin()
                               .AllowAnyMethod()
                               .AllowAnyHeader();
                    });
            });

            var app = builder.Build();

            // Enable CORS
            app.UseCors("AllowAll");

            // Global error handling middleware
            app.Use(async (context, next) =>
            {
                try
                {
                    await next.Invoke();
                }
                catch (Exception ex)
                {
                    context.Response.StatusCode = 500;
                    await context.Response.WriteAsync("An error occurred: " + ex.Message);
                }
            });

            //// Configure the HTTP request pipeline for development
            //if (app.Environment.IsDevelopment())
            //{
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    // Swagger UI settings
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Evacuation API V1");
                    c.RoutePrefix = string.Empty;
                    //c.RoutePrefix = "swagger"; // Set the route prefix for Swagger UI
                });
            //}

            app.UseHttpsRedirection();
            app.UseAuthorization();
            app.MapControllers();
            app.Run();
        }
    }
}
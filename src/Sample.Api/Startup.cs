namespace Sample.Api
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Contracts;
    using MassTransit;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Diagnostics.HealthChecks;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Diagnostics.HealthChecks;
    using Microsoft.Extensions.Hosting;
    using Microsoft.OpenApi.Models;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;


    public class Startup
    {
        static bool? _isRunningInContainer;

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        static bool IsRunningInContainer =>
            _isRunningInContainer ??= bool.TryParse(Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER"), out var inContainer) && inContainer;

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();

            services.AddMassTransit(x =>
            {
                x.AddRequestClient<DispatchRequest>();
                x.AddRequestClient<DispatchResponse>();

                x.UsingRabbitMq((context, cfg) =>
                {
                    cfg.AutoStart = true;

                    if (IsRunningInContainer)
                        cfg.Host("rabbitmq");


                    cfg.ConfigureEndpoints(context);
                });
            });
            services.AddMassTransitHostedService(true);

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Sample.Api",
                    Version = "v1"
                });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Sample.Api v1"));
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseHealthChecks("/health/ready", new HealthCheckOptions
            {
                Predicate = check => check.Tags.Contains("ready"),
                ResponseWriter = HealthCheckResponseWriter
            });
            app.UseHealthChecks("/health/live", new HealthCheckOptions { ResponseWriter = HealthCheckResponseWriter });

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }

        static Task HealthCheckResponseWriter(HttpContext context, HealthReport result)
        {
            var json = new JObject(
                new JProperty("status", result.Status.ToString()),
                new JProperty("results", new JObject(result.Entries.Select(entry => new JProperty(entry.Key, new JObject(
                    new JProperty("status", entry.Value.Status.ToString()),
                    new JProperty("description", entry.Value.Description),
                    new JProperty("data", JObject.FromObject(entry.Value.Data))))))));

            context.Response.ContentType = "application/json";

            return context.Response.WriteAsync(json.ToString(Formatting.Indented));
        }
    }
}

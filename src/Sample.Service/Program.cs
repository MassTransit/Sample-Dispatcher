namespace Sample.Service
{
    using System;
    using System.Reflection;
    using System.Threading.Tasks;
    using Components;
    using Components.Services;
    using Data;
    using MassTransit;
    using MassTransit.RetryPolicies;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Serilog;
    using Serilog.Events;
    using Shared;


    public class Program
    {
        public static async Task Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .MinimumLevel.Override("MassTransit", LogEventLevel.Debug)
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
                .MinimumLevel.Override("Microsoft.EntityFrameworkCore.Database.Command", LogEventLevel.Warning)
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .CreateLogger();

            using var telemetry = ConfigureOpenTelemetryExtensions.AddOpenTelemetry("service");

            var host = CreateHostBuilder(args).Build();

            await CreateDatabase(host);

            await host.RunAsync();
        }

        static async Task CreateDatabase(IHost host)
        {
            await Retry.Exponential(10, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(30), TimeSpan.FromSeconds(5)).Retry(async () =>
            {
                using var scope = host.Services.CreateScope();

                var context = scope.ServiceProvider.GetRequiredService<SampleDbContext>();

                await context.Database.EnsureCreatedAsync();
            });
        }

        static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
                .ConfigureHostConfiguration(config => config.AddEnvironmentVariables())
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddDbContext<SampleDbContext>(builder =>
                    {
                        var connectionString = hostContext.Configuration.GetConnectionString("State");

                        builder.UseNpgsql(connectionString, m =>
                        {
                            m.MigrationsAssembly(Assembly.GetExecutingAssembly().GetName().Name);
                            m.MigrationsHistoryTable($"__{nameof(SampleDbContext)}");
                        });
                    });

                    services.AddSingleton<IServiceEndpointLocator, ServiceEndpointLocator>();
                    services.AddSingleton<IRequestRoutingService, RequestRoutingService>();

                    services.AddRequestRoutingCandidates();
                    services.AddReceiveEndpointOptions(hostContext.Configuration);

                    services.Configure<RabbitMqTransportOptions>(hostContext.Configuration.GetSection("RabbitMqTransport"));

                    services.AddMassTransit(x =>
                    {
                        x.SetEntityFrameworkSagaRepositoryProvider(r =>
                        {
                            r.ExistingDbContext<SampleDbContext>();
                            r.UsePostgres();
                        });

                        var assembly = typeof(ComponentsAssembly).Assembly;

                        x.AddConsumers(assembly);
                        x.AddSagaStateMachines(assembly);
                        x.AddSagas(assembly);
                        x.AddActivities(assembly);

                        x.ConfigureMassTransit();
                    });

                    services.AddOptions<MassTransitHostOptions>().Configure(options =>
                    {
                        options.WaitUntilStarted = true;
                    });
                })
                .UseSerilog();
        }
    }
}

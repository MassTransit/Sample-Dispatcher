namespace Sample.Service
{
    using System;
    using System.Reflection;
    using System.Threading.Tasks;
    using Components;
    using Components.Services;
    using Data;
    using MassTransit;
    using MassTransit.Policies;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Serilog;
    using Serilog.Events;


    public class Program
    {
        static bool? _isRunningInContainer;

        static bool IsRunningInContainer =>
            _isRunningInContainer ??= bool.TryParse(Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER"), out var inContainer) && inContainer;

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

            var host = CreateHostBuilder(args).Build();

            await CreateDatabase(host);

            await host.RunAsync();
        }

        static async Task CreateDatabase(Microsoft.Extensions.Hosting.IHost host)
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
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddDbContext<SampleDbContext>(builder =>
                    {
                        var connectionString = IsRunningInContainer
                            ? "host=postgres;user id=postgres;password=secret;database=SampleService;"
                            : "host=localhost;user id=postgres;password=secret;database=SampleService;";

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

                    services.AddMassTransit(x =>
                    {
                        x.AddDelayedMessageScheduler();

                        x.SetKebabCaseEndpointNameFormatter();

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

                        x.UsingRabbitMq((context, cfg) =>
                        {
                            if (IsRunningInContainer)
                                cfg.Host("rabbitmq");

                            cfg.UseDelayedMessageScheduler();

                            cfg.ConfigureEndpoints(context);
                        });
                    });

                    services.AddMassTransitHostedService(true);
                })
                .UseSerilog();
        }
    }
}

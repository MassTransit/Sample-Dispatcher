namespace Sample.Service
{
    using System;
    using System.Reflection;
    using System.Threading.Tasks;
    using Components;
    using Components.Services;
    using Components.StateMachines;
    using Data;
    using MassTransit;
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
                .MinimumLevel.Error()
                .MinimumLevel.Override("MassTransit", LogEventLevel.Error)
                .MinimumLevel.Override("Microsoft", LogEventLevel.Error)
                .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Fatal)
                .MinimumLevel.Override("Microsoft.EntityFrameworkCore.Database.Command", LogEventLevel.Fatal)
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .CreateLogger();

            var host = CreateHostBuilder(args).Build();

            await CreateDatabase(host);

            await host.RunAsync();
        }

        static async Task CreateDatabase(Microsoft.Extensions.Hosting.IHost host)
        {
            using var scope = host.Services.CreateScope();

            var context = scope.ServiceProvider.GetRequiredService<SampleDbContext>();

            await context.Database.EnsureCreatedAsync();
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

                    services.AddOptions<TransactionStateOptions>();

                    services.AddSingleton<IServiceEndpointLocator, ServiceEndpointLocator>();
                    services.AddSingleton<IRequestRoutingService, RequestRoutingService>();

                    services.AddRequestRoutingCandidates();

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
namespace Sample.Console
{
    using MassTransit;


    public static class Program
    {
        public static async Task Main(string[] args)
        {
            var host = Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddOptions<ConsoleOptions>().Bind(hostContext.Configuration);

                    services.AddHostedService<DispatchRequestWorker>();

                    services.AddMassTransit(x =>
                    {
                        x.UsingRabbitMq((context, cfg) =>
                        {
                            cfg.AutoStart = true;
                        });
                    });
                })
                .Build();

            await host.RunAsync();
        }
    }
}

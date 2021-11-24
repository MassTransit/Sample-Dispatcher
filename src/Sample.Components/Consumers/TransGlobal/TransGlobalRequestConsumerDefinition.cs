namespace Sample.Components.Consumers.TransGlobal
{
    using Configuration;
    using GreenPipes;
    using MassTransit;
    using MassTransit.ConsumeConfigurators;
    using MassTransit.Definition;
    using Microsoft.Extensions.Options;


    public class TransGlobalRequestConsumerDefinition :
        ConsumerDefinition<TransGlobalRequestConsumer>
    {
        readonly TransGlobalOptions _options;

        public TransGlobalRequestConsumerDefinition(IOptions<TransGlobalOptions> options)
        {
            _options = options.Value;
        }

        protected override void ConfigureConsumer(IReceiveEndpointConfigurator endpointConfigurator,
            IConsumerConfigurator<TransGlobalRequestConsumer> consumerConfigurator)
        {
            _options.Configure(endpointConfigurator);

            // messages are only sent directly to this consumer endpoint by the dispatcher
            // this ensures no binding for the message types is created automatically by MassTransit.
            endpointConfigurator.ConfigureConsumeTopology = false;

            endpointConfigurator.UseMessageRetry(r => r.Intervals(10, 50, 100, 500));
            endpointConfigurator.UseInMemoryOutbox();
        }
    }
}
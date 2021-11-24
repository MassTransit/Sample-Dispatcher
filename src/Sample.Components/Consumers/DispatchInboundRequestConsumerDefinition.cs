namespace Sample.Components.Consumers
{
    using GreenPipes;
    using MassTransit;
    using MassTransit.ConsumeConfigurators;
    using MassTransit.Definition;
    using Microsoft.Extensions.Options;
    using StateMachines;


    public class DispatchInboundRequestConsumerDefinition :
        ConsumerDefinition<DispatchInboundRequestConsumer>
    {
        readonly TransactionStateOptions _options;

        public DispatchInboundRequestConsumerDefinition(IOptions<TransactionStateOptions> options)
        {
            _options = options.Value;
        }

        protected override void ConfigureConsumer(IReceiveEndpointConfigurator endpointConfigurator,
            IConsumerConfigurator<DispatchInboundRequestConsumer> consumerConfigurator)
        {
            _options.Configure(endpointConfigurator);

            endpointConfigurator.UseMessageRetry(r => r.Intervals(10, 50, 100, 500));
            endpointConfigurator.UseInMemoryOutbox();
        }
    }
}
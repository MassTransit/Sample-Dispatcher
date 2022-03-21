namespace Sample.Components.Consumers
{
    using Configuration;
    using MassTransit;
    using Microsoft.Extensions.Options;


    public class DispatchRequestConsumerDefinition :
        ConsumerDefinition<DispatchRequestConsumer>
    {
        readonly DispatchOptions _options;

        public DispatchRequestConsumerDefinition(IOptions<DispatchOptions> options)
        {
            _options = options.Value;
        }

        protected override void ConfigureConsumer(IReceiveEndpointConfigurator endpointConfigurator,
            IConsumerConfigurator<DispatchRequestConsumer> consumerConfigurator)
        {
            _options.Configure(endpointConfigurator);

            endpointConfigurator.UseMessageRetry(r => r.Intervals(10, 50, 100, 500));
            endpointConfigurator.UseInMemoryOutbox();
        }
    }
}

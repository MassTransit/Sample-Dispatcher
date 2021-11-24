namespace Sample.Components.Consumers.FirstNational
{
    using Configuration;
    using GreenPipes;
    using MassTransit;
    using MassTransit.ConsumeConfigurators;
    using MassTransit.Definition;
    using Microsoft.Extensions.Options;


    public class FirstNationalRequestConsumerDefinition :
        ConsumerDefinition<FirstNationalRequestConsumer>
    {
        readonly FirstNationalOptions _options;

        public FirstNationalRequestConsumerDefinition(IOptions<FirstNationalOptions> options)
        {
            _options = options.Value;
        }

        protected override void ConfigureConsumer(IReceiveEndpointConfigurator endpointConfigurator,
            IConsumerConfigurator<FirstNationalRequestConsumer> consumerConfigurator)
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

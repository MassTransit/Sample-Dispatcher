namespace Sample.Components.Consumers.FirstNational
{
    using Configuration;
    using MassTransit;
    using Microsoft.Extensions.Options;


    public class FirstNationalResponseConsumerDefinition :
        ConsumerDefinition<FirstNationalResponseConsumer>
    {
        readonly FirstNationalOptions _options;

        public FirstNationalResponseConsumerDefinition(IOptions<FirstNationalOptions> options)
        {
            _options = options.Value;
        }

        protected override void ConfigureConsumer(IReceiveEndpointConfigurator endpointConfigurator,
            IConsumerConfigurator<FirstNationalResponseConsumer> consumerConfigurator)
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

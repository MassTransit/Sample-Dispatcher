namespace Sample.Components.Consumers.FirstNational
{
    using GreenPipes;
    using MassTransit;
    using MassTransit.ConsumeConfigurators;
    using MassTransit.Definition;
    using Microsoft.Extensions.Options;
    using StateMachines;


    public class FirstNationalRequestConsumerDefinition :
        ConsumerDefinition<FirstNationalRequestConsumer>
    {
        readonly TransactionStateOptions _options;

        public FirstNationalRequestConsumerDefinition(IOptions<TransactionStateOptions> options)
        {
            _options = options.Value;
        }

        protected override void ConfigureConsumer(IReceiveEndpointConfigurator endpointConfigurator,
            IConsumerConfigurator<FirstNationalRequestConsumer> consumerConfigurator)
        {
            _options.Configure(endpointConfigurator);

            endpointConfigurator.UseMessageRetry(r => r.Intervals(10, 50, 100, 500));
            endpointConfigurator.UseInMemoryOutbox();
        }
    }
}
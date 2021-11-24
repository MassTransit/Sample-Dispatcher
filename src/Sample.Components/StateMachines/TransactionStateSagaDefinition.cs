namespace Sample.Components.StateMachines
{
    using GreenPipes;
    using MassTransit;
    using MassTransit.Definition;
    using Microsoft.Extensions.Options;


    public class TransactionStateSagaDefinition :
        SagaDefinition<TransactionState>
    {
        readonly TransactionStateOptions _options;

        public TransactionStateSagaDefinition(IOptions<TransactionStateOptions> options)
        {
            _options = options.Value;
        }

        protected override void ConfigureSaga(IReceiveEndpointConfigurator endpointConfigurator, ISagaConfigurator<TransactionState> sagaConfigurator)
        {
            if (_options.PrefetchCount.HasValue)
                endpointConfigurator.PrefetchCount = _options.PrefetchCount.Value;

            if (_options.ConcurrentMessageLimit.HasValue)
                endpointConfigurator.ConcurrentMessageLimit = _options.ConcurrentMessageLimit.Value;


            endpointConfigurator.UseMessageRetry(r => r.Intervals(10, 50, 100, 500));
            endpointConfigurator.UseInMemoryOutbox();
        }
    }
}
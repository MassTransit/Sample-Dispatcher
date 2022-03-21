namespace Sample.Components.StateMachines
{
    using System;
    using Configuration;
    using Contracts;
    using MassTransit;
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

            var partitionCount = endpointConfigurator.PrefetchCount;
            if (endpointConfigurator.ConcurrentMessageLimit.HasValue)
                partitionCount = Math.Min(partitionCount, endpointConfigurator.ConcurrentMessageLimit.Value);

            var partitioner = endpointConfigurator.CreatePartitioner(partitionCount);

            endpointConfigurator.UsePartitioner<DispatchRequest>(partitioner, x => x.Message.TransactionId);
            endpointConfigurator.UsePartitioner<RequestNotDispatched>(partitioner, x => x.Message.TransactionId);
            endpointConfigurator.UsePartitioner<RequestCompleted>(partitioner, x => x.Message.TransactionId);

            endpointConfigurator.UsePartitioner<DispatchResponse>(partitioner, x => x.Message.TransactionId);
            endpointConfigurator.UsePartitioner<ResponseCompleted>(partitioner, x => x.Message.TransactionId);

            endpointConfigurator.UseMessageRetry(r => r.Intervals(10, 50, 100, 500));
            endpointConfigurator.UseInMemoryOutbox();
        }
    }
}

namespace Sample.Components
{
    using System;
    using Consumers;
    using MassTransit;
    using StateMachines;


    public class ServiceEndpointLocator :
        IServiceEndpointLocator
    {
        public ServiceEndpointLocator(IEndpointNameFormatter formatter)
        {
            TransactionStateEndpointAddress = new Uri($"exchange:{formatter.Saga<TransactionState>()}");
            DispatchRequestEndpointAddress = new Uri($"exchange:{formatter.Consumer<DispatchRequestConsumer>()}");
            DispatchResponseEndpointAddress = new Uri($"exchange:{formatter.Saga<TransactionState>()}");
        }

        public Uri TransactionStateEndpointAddress { get; }
        public Uri DispatchRequestEndpointAddress { get; }
        public Uri DispatchResponseEndpointAddress { get; }
    }
}

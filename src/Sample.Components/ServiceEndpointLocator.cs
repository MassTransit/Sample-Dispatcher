namespace Sample.Components
{
    using System;
    using Consumers;
    using MassTransit;
    using StateMachines;


    public class ServiceEndpointLocator :
        IServiceEndpointLocator
    {
        readonly IEndpointNameFormatter _formatter;

        public ServiceEndpointLocator(IEndpointNameFormatter formatter)
        {
            _formatter = formatter;

            TransactionStateEndpointAddress = new Uri($"exchange:{_formatter.Saga<TransactionState>()}");
            DispatchRequestEndpointAddress = new Uri($"exchange:{_formatter.Consumer<DispatchRequestConsumer>()}");
        }

        public Uri TransactionStateEndpointAddress { get; }
        public Uri DispatchRequestEndpointAddress { get; }
    }
}
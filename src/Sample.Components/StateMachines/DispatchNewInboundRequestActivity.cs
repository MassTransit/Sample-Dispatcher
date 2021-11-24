namespace Sample.Components.StateMachines
{
    using System;
    using System.Threading.Tasks;
    using Automatonymous;
    using Contracts;
    using GreenPipes;
    using MassTransit;


    /// <summary>
    /// Forwards the command to the consumer
    /// </summary>
    public class DispatchNewInboundRequestActivity :
        Activity<TransactionState, DispatchRequest>
    {
        readonly IServiceEndpointLocator _locator;

        public DispatchNewInboundRequestActivity(IServiceEndpointLocator locator)
        {
            _locator = locator;
        }

        public void Probe(ProbeContext context)
        {
            context.CreateScope("dispatchNewInboundRequest");
        }

        public void Accept(StateMachineVisitor visitor)
        {
            visitor.Visit(this);
        }

        public async Task Execute(BehaviorContext<TransactionState, DispatchRequest> context, Behavior<TransactionState, DispatchRequest> next)
        {
            var consumeContext = context.GetPayload<ConsumeContext<DispatchRequest>>();

            await consumeContext.Forward(_locator.DispatchRequestEndpointAddress);

            await next.Execute(context);
        }

        public Task Faulted<TException>(BehaviorExceptionContext<TransactionState, DispatchRequest, TException> context,
            Behavior<TransactionState, DispatchRequest> next)
            where TException : Exception
        {
            return next.Faulted(context);
        }
    }
}
namespace Sample.Components.StateMachines
{
    using System;
    using System.Threading.Tasks;
    using Automatonymous;
    using Contracts;
    using GreenPipes;
    using MassTransit;


    public class DispatchNewInboundRequestActivity :
        Activity<TransactionState, DispatchInboundRequest>
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

        public async Task Execute(BehaviorContext<TransactionState, DispatchInboundRequest> context, Behavior<TransactionState, DispatchInboundRequest> next)
        {
            var consumeContext = context.GetPayload<ConsumeContext<DispatchInboundRequest>>();

            await consumeContext.Forward(_locator.DispatchRequestEndpointAddress);

            await next.Execute(context);
        }

        public Task Faulted<TException>(BehaviorExceptionContext<TransactionState, DispatchInboundRequest, TException> context,
            Behavior<TransactionState, DispatchInboundRequest> next)
            where TException : Exception
        {
            return next.Faulted(context);
        }
    }
}
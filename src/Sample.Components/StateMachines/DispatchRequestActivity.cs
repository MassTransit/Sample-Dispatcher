namespace Sample.Components.StateMachines
{
    using System;
    using System.Threading.Tasks;
    using Contracts;
    using MassTransit;


    /// <summary>
    /// Forwards the command to the consumer
    /// </summary>
    public class DispatchRequestActivity :
        IStateMachineActivity<TransactionState, DispatchRequest>
    {
        readonly IServiceEndpointLocator _locator;

        public DispatchRequestActivity(IServiceEndpointLocator locator)
        {
            _locator = locator;
        }

        public void Probe(ProbeContext context)
        {
            context.CreateScope("dispatch-request");
        }

        public void Accept(StateMachineVisitor visitor)
        {
            visitor.Visit(this);
        }

        public async Task Execute(BehaviorContext<TransactionState, DispatchRequest> context, IBehavior<TransactionState, DispatchRequest> next)
        {
            await context.Forward(_locator.DispatchRequestEndpointAddress);

            await next.Execute(context);
        }

        public Task Faulted<TException>(BehaviorExceptionContext<TransactionState, DispatchRequest, TException> context,
            IBehavior<TransactionState, DispatchRequest> next)
            where TException : Exception
        {
            return next.Faulted(context);
        }
    }
}

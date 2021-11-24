namespace Sample.Components.StateMachines
{
    using System;
    using System.Threading.Tasks;
    using Automatonymous;
    using Contracts;
    using GreenPipes;
    using MassTransit;


    public class DispatchResponseActivity :
        Activity<TransactionState, DispatchResponse>
    {
        public void Probe(ProbeContext context)
        {
            context.CreateScope("dispatch-response");
        }

        public void Accept(StateMachineVisitor visitor)
        {
            visitor.Visit(this);
        }

        public async Task Execute(BehaviorContext<TransactionState, DispatchResponse> context, Behavior<TransactionState, DispatchResponse> next)
        {
            var consumeContext = context.GetPayload<ConsumeContext<DispatchResponse>>();

            if (context.Instance.ResponseAddress == null)
                throw new InvalidOperationException("The ResponseAddress was null");

            await consumeContext.Forward(context.Instance.ResponseAddress);

            await next.Execute(context);
        }

        public Task Faulted<TException>(BehaviorExceptionContext<TransactionState, DispatchResponse, TException> context,
            Behavior<TransactionState, DispatchResponse> next)
            where TException : Exception
        {
            return next.Faulted(context);
        }
    }
}

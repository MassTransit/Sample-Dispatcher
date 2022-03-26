namespace Sample.Components.StateMachines
{
    using System;
    using System.Threading.Tasks;
    using Contracts;
    using MassTransit;


    public class DispatchResponseActivity :
        IStateMachineActivity<TransactionState, DispatchResponse>
    {
        public void Probe(ProbeContext context)
        {
            context.CreateScope("dispatch-response");
        }

        public void Accept(StateMachineVisitor visitor)
        {
            visitor.Visit(this);
        }

        public async Task Execute(BehaviorContext<TransactionState, DispatchResponse> context, IBehavior<TransactionState, DispatchResponse> next)
        {
            if (context.Saga.ResponseAddress == null)
                throw new InvalidOperationException("The ResponseAddress was null");

            var endpoint = await context.GetSendEndpoint(context.Saga.ResponseAddress);

            await endpoint.Send(new DispatchResponse
            {
                TransactionId = context.Message.TransactionId,
                Body = context.Message.Body,
                RequestBody = context.Saga.RequestBody,
                ResponseTimestamp = context.Message.ResponseTimestamp
            }, sc =>
            {
                sc.MessageId = context.MessageId;
                sc.RequestId = context.RequestId;
                sc.ConversationId = context.ConversationId;
                sc.CorrelationId = context.CorrelationId;
                sc.InitiatorId = context.InitiatorId;
                sc.SourceAddress = context.SourceAddress;
                sc.ResponseAddress = context.ResponseAddress;
                sc.FaultAddress = context.FaultAddress;

                if (context.ExpirationTime.HasValue)
                    sc.TimeToLive = context.ExpirationTime.Value.ToUniversalTime() - DateTime.UtcNow;

                foreach (var (key, value) in context.Headers.GetAll())
                    sc.Headers.Set(key, value);
            });

            await next.Execute(context);
        }

        public Task Faulted<TException>(BehaviorExceptionContext<TransactionState, DispatchResponse, TException> context,
            IBehavior<TransactionState, DispatchResponse> next)
            where TException : Exception
        {
            return next.Faulted(context);
        }
    }
}

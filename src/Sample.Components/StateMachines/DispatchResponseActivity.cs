namespace Sample.Components.StateMachines
{
    using System;
    using System.Collections.Generic;
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

            var endpoint = await context.GetSendEndpoint(context.Instance.ResponseAddress);

            await endpoint.Send(new DispatchResponse
            {
                TransactionId = context.Data.TransactionId,
                Body = context.Data.Body,
                RequestBody = context.Instance.RequestBody,
                ResponseTimestamp = context.Data.ResponseTimestamp
            }, sc =>
            {
                sc.MessageId = consumeContext.MessageId;
                sc.RequestId = consumeContext.RequestId;
                sc.ConversationId = consumeContext.ConversationId;
                sc.CorrelationId = consumeContext.CorrelationId;
                sc.InitiatorId = consumeContext.InitiatorId;
                sc.SourceAddress = consumeContext.SourceAddress;
                sc.ResponseAddress = consumeContext.ResponseAddress;
                sc.FaultAddress = consumeContext.FaultAddress;

                if (consumeContext.ExpirationTime.HasValue)
                    sc.TimeToLive = consumeContext.ExpirationTime.Value.ToUniversalTime() - DateTime.UtcNow;

                foreach (KeyValuePair<string, object> header in consumeContext.Headers.GetAll())
                    sc.Headers.Set(header.Key, header.Value);
            });

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

namespace Sample.Components.StateMachines
{
    using System;
    using Automatonymous;
    using Contracts;
    using MassTransit;
    using Microsoft.Extensions.Logging;


    public class TransactionStateMachine :
        MassTransitStateMachine<TransactionState>
    {
        public TransactionStateMachine(ILogger<TransactionStateMachine> logger)
        {
            InstanceState(x => x.CurrentState);

            Event(() => DispatchInbound, x =>
            {
                x.CorrelateBy(state => state.TransactionId, m => m.Message.TransactionId)
                    .SelectId(context => context.MessageId ?? NewId.NextGuid());

                x.InsertOnInitial = true;
                x.SetSagaFactory(context => new TransactionState
                {
                    CorrelationId = context.MessageId ?? NewId.NextGuid(),
                    TransactionId = context.Message.TransactionId,
                    Created = DateTime.UtcNow,
                    Received = context.Message.ReceiveTimestamp,
                    Deadline = context.ExpirationTime
                });
            });

            Initially(
                When(DispatchInbound)
                    .Then(x => logger.LogDebug("Transaction {TransactionId} received, deadline {Deadline}", x.Instance.TransactionId, x.Instance.Deadline))
                    .Activity(x => x.OfType<DispatchNewInboundRequestActivity>())
                    .TransitionTo(DispatchingRequest)
            );
        }

        public State DispatchingRequest { get; } = null!;

        public Event<DispatchInboundRequest> DispatchInbound { get; } = null!;
    }
}
namespace Sample.Components.StateMachines
{
    using System;
    using Automatonymous;
    using Automatonymous.Binders;
    using Contracts;
    using GreenPipes;
    using MassTransit;
    using Microsoft.Extensions.Logging;


    public class TransactionStateMachine :
        MassTransitStateMachine<TransactionState>
    {
        public TransactionStateMachine(ILogger<TransactionStateMachine> logger)
        {
            InstanceState(x => x.CurrentState);

            Event(() => DispatchRequest, x =>
            {
                x.CorrelateBy(state => state.TransactionId, m => m.Message.TransactionId)
                    .SelectId(context => context.MessageId ?? NewId.NextGuid());

                // do not bind the exchange for this message type, only support direct send
                x.ConfigureConsumeTopology = false;
            });

            Event(() => RequestNotDispatched, x =>
            {
                x.CorrelateBy(state => state.TransactionId, m => m.Message.TransactionId)
                    .SelectId(context => context.Message.RequestMessageId ?? context.MessageId ?? NewId.NextGuid());

                x.OnMissingInstance(m => m.Discard());
            });

            Event(() => RequestCompleted, x =>
            {
                x.CorrelateBy(state => state.TransactionId, m => m.Message.TransactionId)
                    .SelectId(context => context.Message.RequestMessageId ?? context.MessageId ?? NewId.NextGuid());
            });

            Initially(
                // Since an existing saga instance does not exist, this is a new transaction, so forward
                // it directly to the request dispatch consumer
                When(DispatchRequest)
                    .InitializeInstance()
                    .Then(x => logger.LogDebug("Dispatching {TransactionId}, deadline {Deadline}", x.Instance.TransactionId, x.Instance.Deadline))
                    .Activity(x => x.OfType<DispatchNewInboundRequestActivity>())
                    .TransitionTo(RequestDispatching)
            );

            During(Initial, RequestDispatching,
                When(RequestCompleted)
                    .InitializeInstance()
                    .Then(context =>
                    {
                        context.Instance.RequestCompleted = context.Data.CompletedTimestamp;
                    })
                    .TransitionTo(RequestComplete)
            );

            During(RequestDispatching,
                When(RequestNotDispatched)
                    .Finalize());

            During(RequestComplete,
                Ignore(DispatchRequest) // may want to play this differently at some point...
            );

            SetCompletedWhenFinalized();
        }

        //
        // ReSharper disable MemberCanBePrivate.Global
        public State RequestDispatching { get; } = null!;
        public State RequestComplete { get; } = null!;

        public Event<DispatchRequest> DispatchRequest { get; } = null!;
        public Event<RequestNotDispatched> RequestNotDispatched { get; } = null!;
        public Event<RequestCompleted> RequestCompleted { get; } = null!;
    }


    public static class TransactionStateMachineExtensions
    {
        public static EventActivityBinder<TransactionState, DispatchRequest> InitializeInstance(
            this EventActivityBinder<TransactionState, DispatchRequest> binder)
        {
            return binder.Then(context =>
            {
                var consumeContext = context.GetPayload<ConsumeContext>();

                context.Instance.TransactionId = context.Data.TransactionId;
                context.Instance.Created = DateTime.UtcNow;
                context.Instance.RequestReceived = context.Data.ReceiveTimestamp;
                context.Instance.Deadline = consumeContext.ExpirationTime;
            });
        }

        public static EventActivityBinder<TransactionState, T> InitializeInstance<T>(this EventActivityBinder<TransactionState, T> binder)
            where T : RequestEvent
        {
            return binder.Then(context =>
            {
                context.Instance.TransactionId = context.Data.TransactionId;
                context.Instance.Created = DateTime.UtcNow;
                context.Instance.RequestReceived = context.Data.ReceiveTimestamp;
                context.Instance.Deadline = context.Data.Deadline;
            });
        }
    }
}
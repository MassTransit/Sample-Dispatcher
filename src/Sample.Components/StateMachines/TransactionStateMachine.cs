﻿namespace Sample.Components.StateMachines
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
            InstanceState(x => x.CurrentState, RequestInFlight, RequestComplete, ResponseInFlight, ResponseComplete);

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

            Event(() => ResponseCompleted, x =>
            {
                x.CorrelateBy(state => state.TransactionId, m => m.Message.TransactionId);
            });

            Event(() => DispatchResponse, x =>
            {
                x.CorrelateBy(state => state.TransactionId, m => m.Message.TransactionId);
                x.OnMissingInstance(m => m.ExecuteAsync(context => context.RespondAsync(new DispatchResponseCompleted
                {
                    TransactionId = context.Message.TransactionId,
                    Body = context.Message.Body,
                    CompletedTimestamp = DateTime.UtcNow
                })));
            });

            Initially(
                // Since an existing saga instance does not exist, this is a new transaction, so forward
                // it directly to the request dispatch consumer
                When(DispatchRequest)
                    .InitializeInstance()
                    .Then(x => logger.LogDebug("Dispatching {TransactionId}, deadline {Deadline}", x.Instance.TransactionId, x.Instance.Deadline))
                    .Activity(x => x.OfType<DispatchRequestActivity>())
                    .TransitionTo(RequestInFlight)
            );

            During(Initial, RequestInFlight,
                When(RequestCompleted)
                    .InitializeInstance()
                    .Then(context =>
                    {
                        context.Instance.RequestCompleted = context.Data.CompletedTimestamp;
                        context.Instance.ResponseAddress = context.Data.ResponseAddress;
                    })
                    .TransitionTo(RequestComplete)
            );

            During(RequestInFlight,
                When(RequestNotDispatched)
                    .Finalize()
            );

            During(RequestComplete,
                Ignore(DispatchRequest) // may want to play this differently at some point...
            );

            // Response handling

            During(RequestComplete,
                When(DispatchResponse)
                    .IfElse(context => context.Instance.ResponseAddress != null,
                        dispatchResponse => dispatchResponse.Activity(x => x.OfType<DispatchResponseActivity>()).TransitionTo(ResponseInFlight),
                        complete => complete.TransitionTo(ResponseComplete))
            );

            During(ResponseInFlight,
                When(ResponseCompleted)
                    .TransitionTo(ResponseComplete)
            );

            SetCompletedWhenFinalized();
        }

        //
        // ReSharper disable MemberCanBePrivate.Global
        // ReSharper disable UnassignedGetOnlyAutoProperty

    #nullable disable
        public State RequestInFlight { get; }
        public State RequestComplete { get; }
        public State ResponseInFlight { get; }
        public State ResponseComplete { get; }

        public Event<DispatchRequest> DispatchRequest { get; }
        public Event<RequestNotDispatched> RequestNotDispatched { get; }
        public Event<RequestCompleted> RequestCompleted { get; }

        public Event<DispatchResponse> DispatchResponse { get; }
        public Event<ResponseCompleted> ResponseCompleted { get; }
    #nullable enable
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
                context.Instance.RequestReceived = context.Data.RequestTimestamp;
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

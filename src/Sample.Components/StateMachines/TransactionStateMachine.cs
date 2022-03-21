namespace Sample.Components.StateMachines
{
    using System;
    using Contracts;
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
                    .Then(x => logger.LogDebug("Dispatching {TransactionId}, deadline {Deadline}", x.Saga.TransactionId, x.Saga.Deadline))
                    .Activity(x => x.OfType<DispatchRequestActivity>())
                    .TransitionTo(RequestInFlight)
            );

            During(Initial, RequestInFlight,
                When(RequestCompleted)
                    .InitializeInstance()
                    .Then(context =>
                    {
                        context.Saga.RequestCompleted = context.Message.CompletedTimestamp;
                        context.Saga.ResponseAddress = context.Message.ResponseAddress;
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
                    .Then(context => context.Saga.ResponseBody = context.Message.Body)
                    .IfElse(context => context.Saga.ResponseAddress != null,
                        dispatch => dispatch
                            .Activity(x => x.OfType<DispatchResponseActivity>())
                            .TransitionTo(ResponseInFlight),
                        complete => complete
                            .Respond(context => new DispatchResponseCompleted
                            {
                                TransactionId = context.Message.TransactionId,
                                Body = context.Message.Body,
                                CompletedTimestamp = DateTime.UtcNow
                            })
                            .TransitionTo(ResponseComplete))
            );

            During(ResponseInFlight,
                When(ResponseCompleted)
                    .TransitionTo(ResponseComplete)
            );

            During(ResponseComplete,
                Ignore(ResponseCompleted)
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

                context.Saga.TransactionId = context.Message.TransactionId;
                context.Saga.Created = DateTime.UtcNow;
                context.Saga.RequestReceived = context.Message.RequestTimestamp;
                context.Saga.Deadline = consumeContext.ExpirationTime;
                context.Saga.RequestBody = context.Message.Body;
            });
        }

        public static EventActivityBinder<TransactionState, T> InitializeInstance<T>(this EventActivityBinder<TransactionState, T> binder)
            where T : RequestEvent
        {
            return binder.Then(context =>
            {
                context.Saga.TransactionId = context.Message.TransactionId;
                context.Saga.Created = DateTime.UtcNow;
                context.Saga.RequestReceived = context.Message.ReceiveTimestamp;
                context.Saga.Deadline = context.Message.Deadline;
                context.Saga.RequestBody = context.Message.Body;
            });
        }
    }
}

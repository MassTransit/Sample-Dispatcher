namespace Sample.Components.Consumers
{
    using System;
    using System.Threading.Tasks;
    using Contracts;
    using MassTransit;
    using Services;


    public class DispatchRequestConsumer :
        IConsumer<DispatchRequest>
    {
        readonly IServiceEndpointLocator _endpointLocator;
        readonly IRequestRoutingService _routingService;

        public DispatchRequestConsumer(IRequestRoutingService routingService, IServiceEndpointLocator endpointLocator)
        {
            _routingService = routingService;
            _endpointLocator = endpointLocator;
        }

        public async Task Consume(ConsumeContext<DispatchRequest> context)
        {
            // When redelivered, push through the state machine to avoid duplicate transactions
            if (context.ReceiveContext.Redelivered)
            {
                await context.Forward(_endpointLocator.TransactionStateEndpointAddress);
                return;
            }

            var request = context.Message;
            var routeResult = await _routingService.RouteRequest(request.RoutingKey);

            if (routeResult.Disposition is RouteDisposition.Unhandled or RouteDisposition.Ambiguous)
            {
                var completedTimestamp = DateTime.UtcNow;

                // response with the unmodified body to the request
                await context.RespondAsync(new DispatchRequestCompleted
                {
                    TransactionId = context.Message.TransactionId,
                    RoutingKey = context.Message.RoutingKey,
                    Body = context.Message.Body,
                    CompletedTimestamp = completedTimestamp
                });

                // publish to remove any pending state machine instance since not dispatched requests
                // should not handle a response
                await context.Publish(new RequestNotDispatched
                {
                    RequestMessageId = context.MessageId,
                    TransactionId = context.Message.TransactionId,
                    RoutingKey = context.Message.RoutingKey,
                    ReceiveTimestamp = context.Message.RequestTimestamp,
                    Deadline = context.ExpirationTime
                });

                return;
            }

            await context.Forward(routeResult.DestinationAddress);
        }
    }
}

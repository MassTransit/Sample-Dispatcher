namespace Sample.Components.Consumers.FirstNational
{
    using System;
    using System.Threading.Tasks;
    using Contracts;
    using MassTransit;


    public class FirstNationalRequestConsumer :
        IConsumer<DispatchRequest>
    {
        readonly Uri _responseAddress;

        public FirstNationalRequestConsumer(IEndpointNameFormatter formatter)
        {
            _responseAddress = new Uri($"exchange:{formatter.Consumer<FirstNationalResponseConsumer>()}");
        }

        public async Task Consume(ConsumeContext<DispatchRequest> context)
        {
            var timestamp = DateTime.UtcNow;

            await context.RespondAsync(new DispatchRequestCompleted
            {
                TransactionId = context.Message.TransactionId,
                RoutingKey = context.Message.RoutingKey,
                Body = $"First National Request: {context.Message.Body}",
                CompletedTimestamp = timestamp
            });

            await context.Publish(new RequestCompleted
            {
                TransactionId = context.Message.TransactionId,
                RoutingKey = context.Message.RoutingKey,
                ReceiveTimestamp = context.Message.RequestTimestamp,
                RequestMessageId = context.MessageId,
                CompletedTimestamp = timestamp,
                ResponseAddress = _responseAddress
            });
        }
    }
}

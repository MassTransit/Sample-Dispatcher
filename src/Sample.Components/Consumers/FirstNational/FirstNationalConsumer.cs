namespace Sample.Components.Consumers.FirstNational
{
    using System;
    using System.Threading.Tasks;
    using Contracts;
    using MassTransit;


    public class FirstNationalRequestConsumer :
        IConsumer<DispatchRequest>
    {
        public async Task Consume(ConsumeContext<DispatchRequest> context)
        {
            var completedTimestamp = DateTime.UtcNow;

            await context.RespondAsync(new DispatchInboundRequestCompleted
            {
                TransactionId = context.Message.TransactionId,
                RoutingKey = context.Message.RoutingKey,
                Body = $"First National: {context.Message.Body}",
                CompletedTimestamp = completedTimestamp
            });

            await context.Publish(new RequestCompleted
            {
                TransactionId = context.Message.TransactionId,
                RoutingKey = context.Message.RoutingKey,
                ReceiveTimestamp = context.Message.ReceiveTimestamp,
                RequestMessageId = context.MessageId,
                CompletedTimestamp = completedTimestamp,
            });
        }
    }
}
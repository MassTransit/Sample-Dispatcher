namespace Sample.Components.Consumers.FirstNational
{
    using System;
    using System.Threading.Tasks;
    using Contracts;
    using MassTransit;


    public class FirstNationalRequestConsumer :
        IConsumer<DispatchInboundRequest>
    {
        public async Task Consume(ConsumeContext<DispatchInboundRequest> context)
        {
            await context.RespondAsync(new DispatchInboundRequestCompleted
            {
                TransactionId = context.Message.TransactionId,
                RoutingKey = context.Message.RoutingKey,
                Body = context.Message.Body,
                CompletedTimestamp = DateTime.UtcNow
            });
        }
    }
}
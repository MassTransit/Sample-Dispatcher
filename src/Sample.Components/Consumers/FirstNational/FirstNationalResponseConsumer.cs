namespace Sample.Components.Consumers.FirstNational
{
    using System;
    using System.Threading.Tasks;
    using Contracts;
    using MassTransit;


    public class FirstNationalResponseConsumer :
        IConsumer<DispatchResponse>
    {
        public async Task Consume(ConsumeContext<DispatchResponse> context)
        {
            var timestamp = DateTime.UtcNow;

            await context.RespondAsync(new DispatchResponseCompleted
            {
                TransactionId = context.Message.TransactionId,
                Body = $"First National Response: {context.Message.Body}",
                CompletedTimestamp = timestamp
            });

            await context.Publish(new ResponseCompleted
            {
                TransactionId = context.Message.TransactionId,
                CompletedTimestamp = timestamp,
            });
        }
    }
}

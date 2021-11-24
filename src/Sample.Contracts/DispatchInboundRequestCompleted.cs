namespace Sample.Contracts
{
    using System;


    public record DispatchInboundRequestCompleted
    {
        public string TransactionId { get; init; }

        public string RoutingKey { get; init; }

        public string Body { get; init; }

        public DateTime CompletedTimestamp { get; init; }
    }
}
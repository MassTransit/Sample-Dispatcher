namespace Sample.Contracts
{
    using System;


    public record DispatchResponseCompleted
    {
        public string? TransactionId { get; init; }

        public string? Body { get; init; }

        public DateTime? CompletedTimestamp { get; init; }
    }
}

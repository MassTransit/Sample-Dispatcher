namespace Sample.Contracts
{
    using System;


    public record ResponseCompleted :
        ResponseEvent
    {
        public DateTime? CompletedTimestamp { get; init; }
    }
}

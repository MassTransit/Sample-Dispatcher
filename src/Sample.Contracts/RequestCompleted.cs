namespace Sample.Contracts
{
    using System;


    public record RequestCompleted :
        RequestEvent
    {
        public DateTime? CompletedTimestamp { get; init; }

        public Uri? ResponseAddress { get; init; }
    }
}

namespace Sample.Components.StateMachines
{
    public record TransactionStateOptions :
        IReceiveEndpointOptions
    {
        public int? PrefetchCount { get; init; }
        public int? ConcurrentMessageLimit { get; init; }
    }
}
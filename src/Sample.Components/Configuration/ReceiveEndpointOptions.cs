namespace Sample.Components.Configuration
{
    /// <summary>
    /// Common configuration options used by consumers, state machines, etc. which can be
    /// configured by creating a subclass with the name SomethingOptions where those options
    /// will be read from Endpoint:Something in the configuration.
    /// </summary>
    public record ReceiveEndpointOptions
    {
        public int? PrefetchCount { get; init; }
        public int? ConcurrentMessageLimit { get; init; }
    }
}
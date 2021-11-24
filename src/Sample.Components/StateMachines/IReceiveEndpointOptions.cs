namespace Sample.Components.StateMachines
{
    using MassTransit;


    public interface IReceiveEndpointOptions
    {
        int? PrefetchCount { get; }
        int? ConcurrentMessageLimit { get; }
    }


    public static class ReceiveEndpointOptionsExtensions
    {
        public static void Configure(this IReceiveEndpointOptions options, IReceiveEndpointConfigurator configurator)
        {
            if (options.PrefetchCount.HasValue)
                configurator.PrefetchCount = options.PrefetchCount.Value;

            if (options.ConcurrentMessageLimit.HasValue)
                configurator.ConcurrentMessageLimit = options.ConcurrentMessageLimit.Value;
        }
    }
}
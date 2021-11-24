namespace Sample.Components.Configuration
{
    using MassTransit;


    public static class ReceiveEndpointOptionsExtensions
    {
        /// <summary>
        /// Uses the options to configure the receive endpoint settings (PrefetchCount, ConcurrentMessageLimit)
        /// </summary>
        /// <param name="options"></param>
        /// <param name="configurator"></param>
        public static void Configure(this ReceiveEndpointOptions options, IReceiveEndpointConfigurator configurator)
        {
            if (options.PrefetchCount.HasValue)
                configurator.PrefetchCount = options.PrefetchCount.Value;

            if (options.ConcurrentMessageLimit.HasValue)
                configurator.ConcurrentMessageLimit = options.ConcurrentMessageLimit.Value;
        }
    }
}
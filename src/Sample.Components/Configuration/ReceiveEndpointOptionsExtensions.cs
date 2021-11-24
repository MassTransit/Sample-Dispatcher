namespace Sample.Components.Configuration
{
    using System;
    using System.Text;
    using GreenPipes.Partitioning;
    using GreenPipes.Specifications;
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

        /// <summary>
        /// Specify a concurrency limit for tasks executing through the filter. No more than the specified
        /// number of tasks will be allowed to execute concurrently.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="configurator"></param>
        /// <param name="partitioner">An existing partitioner that is shared</param>
        /// <param name="keyProvider">Provides the key from the message</param>
        /// <param name="encoding"></param>
        public static void UsePartitioner<T>(this IConsumePipeConfigurator configurator, IPartitioner partitioner, Func<ConsumeContext<T>, string?> keyProvider,
            Encoding? encoding = null)
            where T : class
        {
            if (configurator == null)
                throw new ArgumentNullException(nameof(configurator));
            if (partitioner == null)
                throw new ArgumentNullException(nameof(partitioner));
            if (keyProvider == null)
                throw new ArgumentNullException(nameof(keyProvider));

            var textEncoding = encoding ?? Encoding.UTF8;

            byte[] Provider(ConsumeContext<T> context)
            {
                var key = keyProvider(context);
                return key == null
                    ? Array.Empty<byte>()
                    : textEncoding.GetBytes(key);
            }

            var specification = new PartitionerPipeSpecification<ConsumeContext<T>>(Provider, partitioner);

            configurator.AddPipeSpecification(specification);
        }
    }
}

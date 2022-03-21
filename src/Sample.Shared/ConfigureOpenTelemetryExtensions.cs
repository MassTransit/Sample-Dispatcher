namespace Sample.Shared
{
    using System.Diagnostics;
    using OpenTelemetry;
    using OpenTelemetry.Resources;
    using OpenTelemetry.Trace;


    public static class ConfigureOpenTelemetryExtensions
    {
        public static TracerProvider AddOpenTelemetry(string serviceName)
        {
            return Sdk.CreateTracerProviderBuilder()
                .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(serviceName))
                .AddSource("MassTransit")
                .AddJaegerExporter(o =>
                {
                    o.AgentHost = SampleConfigurationExtensions.IsRunningInContainer ? "jaeger" : "localhost";
                    o.AgentPort = 6831;
                    o.MaxPayloadSizeInBytes = 4096;
                    o.ExportProcessorType = ExportProcessorType.Batch;
                    o.BatchExportProcessorOptions = new BatchExportProcessorOptions<Activity>
                    {
                        MaxQueueSize = 2048,
                        ScheduledDelayMilliseconds = 5000,
                        ExporterTimeoutMilliseconds = 30000,
                        MaxExportBatchSize = 512,
                    };
                })
                .Build();
        }
    }
}

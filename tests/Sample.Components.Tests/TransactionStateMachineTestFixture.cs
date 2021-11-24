namespace Sample.Components.Tests
{
    using Consumers;
    using MassTransit;
    using MassTransit.ExtensionsDependencyInjectionIntegration;
    using Microsoft.Extensions.DependencyInjection;
    using Services;
    using StateMachines;


    public class TransactionStateMachineTestFixture :
        StateMachineTestFixture<TransactionStateMachine, TransactionState, TransactionStateSagaDefinition>
    {
        protected override void ConfigureMassTransit(IServiceCollectionBusConfigurator configurator)
        {
            base.ConfigureMassTransit(configurator);

            configurator.AddConsumer<DispatchRequestConsumer, DispatchRequestConsumerDefinition>();
        }

        protected override void ConfigureServices(IServiceCollection collection)
        {
            base.ConfigureServices(collection);

            collection.AddReceiveEndpointOptions();

            collection.AddSingleton<IServiceEndpointLocator, ServiceEndpointLocator>();

            collection.AddSingleton<IRequestRoutingService, RequestRoutingService>();
        }
    }
}
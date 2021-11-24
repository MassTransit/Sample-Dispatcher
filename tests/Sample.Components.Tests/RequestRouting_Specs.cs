namespace Sample.Components.Tests
{
    using System;
    using System.Threading.Tasks;
    using Consumers.FirstNational;
    using Contracts;
    using MassTransit;
    using MassTransit.ExtensionsDependencyInjectionIntegration;
    using Microsoft.Extensions.DependencyInjection;
    using NUnit.Framework;
    using Services;


    public class Routing_a_dispatch_request :
        TransactionStateMachineTestFixture
    {
        [Test]
        public async Task Should_support_the_status_check()
        {
            var transactionId = NewId.NextGuid().ToString("N");
            var receiveTimestamp = DateTime.UtcNow;

            var locator = Provider.GetRequiredService<IServiceEndpointLocator>();

            IRequestClient<DispatchInboundRequest> requestClient =
                TestHarness.Bus.CreateRequestClient<DispatchInboundRequest>(locator.DispatchRequestEndpointAddress, RequestTimeout.After(s: 5));

            Response<DispatchInboundRequestCompleted> response = await requestClient.GetResponse<DispatchInboundRequestCompleted>(new DispatchInboundRequest
            {
                TransactionId = transactionId,
                ReceiveTimestamp = receiveTimestamp,
                RoutingKey = "FIRSTNATL"
            });

            var consumed = await TestHarness.Consumed.Any<DispatchInboundRequest>(x => x.Context.Message.TransactionId == transactionId);
            Assert.IsTrue(consumed, "DispatchInboundRequest not consumed");
        }

        protected override void ConfigureMassTransit(IServiceCollectionBusConfigurator configurator)
        {
            base.ConfigureMassTransit(configurator);

            configurator.AddConsumer<FirstNationalRequestConsumer, FirstNationalRequestConsumerDefinition>();
        }

        protected override void ConfigureServices(IServiceCollection collection)
        {
            base.ConfigureServices(collection);

            collection.AddSingleton<IRequestRoutingCandidate, FirstNationalRoutingCandidate>();
        }
    }
}
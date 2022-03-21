namespace Sample.Components.Tests
{
    using System;
    using System.Threading.Tasks;
    using Consumers.FirstNational;
    using Contracts;
    using MassTransit;
    using Microsoft.Extensions.DependencyInjection;
    using NUnit.Framework;
    using Services;


    public class Routing_a_dispatch_request :
        TransactionStateMachineTestFixture
    {
        [Test]
        public async Task Should_dispatch_to_the_consumer()
        {
            var transactionId = NewId.NextGuid().ToString("N");
            var receiveTimestamp = DateTime.UtcNow;

            var locator = Provider.GetRequiredService<IServiceEndpointLocator>();

            IRequestClient<DispatchRequest> requestClient =
                TestHarness.Bus.CreateRequestClient<DispatchRequest>(locator.DispatchRequestEndpointAddress, RequestTimeout.After(s: 5));

            Response<DispatchRequestCompleted> response = await requestClient.GetResponse<DispatchRequestCompleted>(new DispatchRequest
            {
                TransactionId = transactionId,
                RequestTimestamp = receiveTimestamp,
                RoutingKey = "FIRSTNATL"
            });

            var published = await TestHarness.Published.Any<RequestCompleted>(x => x.Context.Message.TransactionId == transactionId);
            Assert.IsTrue(published);

            await TestHarness.InactivityTask;
        }

        [Test]
        public async Task Should_not_dispatch_an_unsupported_request()
        {
            var transactionId = NewId.NextGuid().ToString("N");
            var receiveTimestamp = DateTime.UtcNow;

            var locator = Provider.GetRequiredService<IServiceEndpointLocator>();

            IRequestClient<DispatchRequest> requestClient =
                TestHarness.Bus.CreateRequestClient<DispatchRequest>(locator.DispatchRequestEndpointAddress, RequestTimeout.After(s: 5));

            Response<DispatchRequestCompleted> response = await requestClient.GetResponse<DispatchRequestCompleted>(new DispatchRequest
            {
                TransactionId = transactionId,
                RequestTimestamp = receiveTimestamp,
                RoutingKey = "TACOTUESDAY"
            });

            var published = await TestHarness.Published.Any<RequestNotDispatched>(x => x.Context.Message.TransactionId == transactionId);
            Assert.IsTrue(published);

            await TestHarness.InactivityTask;
        }

        protected override void ConfigureMassTransit(IBusRegistrationConfigurator configurator)
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

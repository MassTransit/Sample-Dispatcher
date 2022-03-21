namespace Sample.Components.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Consumers.FirstNational;
    using Contracts;
    using MassTransit;
    using MassTransit.Testing;
    using Microsoft.Extensions.DependencyInjection;
    using NUnit.Framework;
    using Services;


    public class Routing_a_dispatch_request_and_response :
        TransactionStateMachineTestFixture
    {
        [Test]
        public async Task Should_match_the_request_and_response()
        {
            var transactionId = NewId.NextGuid().ToString("N");
            var receiveTimestamp = DateTime.UtcNow;

            var locator = Provider.GetRequiredService<IServiceEndpointLocator>();

            IRequestClient<DispatchRequest> requestClient =
                TestHarness.Bus.CreateRequestClient<DispatchRequest>(locator.DispatchRequestEndpointAddress, RequestTimeout.After(s: 5));

            await requestClient.GetResponse<DispatchRequestCompleted>(new DispatchRequest
            {
                TransactionId = transactionId,
                RequestTimestamp = receiveTimestamp,
                RoutingKey = "FIRSTNATL"
            });

            Assert.IsTrue(await TestHarness.Published.Any<RequestCompleted>(x => x.Context.Message.TransactionId == transactionId));

            IList<Guid> correlationIds = await SagaHarness.Exists(state => state.TransactionId == transactionId, Machine.RequestComplete);
            Assert.That(correlationIds, Is.Not.Null.Or.Empty);

            IRequestClient<DispatchResponse> responseClient =
                TestHarness.Bus.CreateRequestClient<DispatchResponse>(locator.DispatchResponseEndpointAddress, RequestTimeout.After(s: 5));

            await responseClient.GetResponse<DispatchResponseCompleted>(new DispatchResponse
            {
                TransactionId = transactionId,
                ResponseTimestamp = DateTime.UtcNow,
            });

            Assert.IsTrue(await TestHarness.Published.Any<ResponseCompleted>(x => x.Context.Message.TransactionId == transactionId));

            await TestHarness.InactivityTask;

            await TestHarness.OutputTimeline(TestContext.Out);
        }

        protected override void ConfigureMassTransit(IBusRegistrationConfigurator configurator)
        {
            base.ConfigureMassTransit(configurator);

            configurator.AddConsumer<FirstNationalRequestConsumer, FirstNationalRequestConsumerDefinition>();
            configurator.AddConsumer<FirstNationalResponseConsumer, FirstNationalResponseConsumerDefinition>();
        }

        protected override void ConfigureServices(IServiceCollection collection)
        {
            base.ConfigureServices(collection);

            collection.AddSingleton<IRequestRoutingCandidate, FirstNationalRoutingCandidate>();
        }
    }
}

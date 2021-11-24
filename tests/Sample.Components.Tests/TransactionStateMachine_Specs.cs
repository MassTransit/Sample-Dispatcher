namespace Sample.Components.Tests
{
    namespace RequestTransaction
    {
        using System;
        using System.Collections.Generic;
        using System.Threading.Tasks;
        using Contracts;
        using MassTransit;
        using Microsoft.Extensions.DependencyInjection;
        using NUnit.Framework;


        public class Receiving_a_new_inbound_request :
            TransactionStateMachineTestFixture
        {
            [Test]
            public async Task Should_transition_to_dispatching()
            {
                var transactionId = NewId.NextGuid().ToString("N");
                var receiveTimestamp = DateTime.UtcNow;

                var endpoint = await TestHarness.Bus.GetSendEndpoint(Provider.GetRequiredService<IServiceEndpointLocator>().TransactionStateEndpointAddress);

                await endpoint.Send(new DispatchRequest
                {
                    TransactionId = transactionId,
                    RequestTimestamp = receiveTimestamp,
                    RoutingKey = "ABC"
                }, s => s.TimeToLive = TimeSpan.FromSeconds(5));

                Assert.IsTrue(await TestHarness.Consumed.Any<DispatchRequest>(x => x.Context.Message.TransactionId == transactionId),
                    "DispatchInboundRequest not consumed");

                IList<Guid> correlationIds = await SagaHarness.Exists(state => state.TransactionId == transactionId, Machine.RequestInFlight);
                Assert.That(correlationIds, Is.Not.Null.Or.Empty);

                await TestHarness.InactivityTask;
            }
        }
    }
}

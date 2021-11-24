namespace Sample.Components.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Contracts;
    using MassTransit;
    using Microsoft.Extensions.DependencyInjection;
    using NUnit.Framework;


    namespace RequestTransaction
    {
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
                    ReceiveTimestamp = receiveTimestamp,
                    RoutingKey = "ABC"
                }, s => s.TimeToLive = TimeSpan.FromSeconds(5));

                var consumed = await TestHarness.Consumed.Any<DispatchRequest>(x => x.Context.Message.TransactionId == transactionId);
                Assert.IsTrue(consumed, "DispatchInboundRequest not consumed");

                IList<Guid> correlationIds = await SagaHarness.Exists(state => state.TransactionId == transactionId, Machine.RequestDispatching);
                Assert.That(correlationIds, Is.Not.Null.Or.Empty);

                await TestHarness.InactivityTask;
            }
        }
    }
}
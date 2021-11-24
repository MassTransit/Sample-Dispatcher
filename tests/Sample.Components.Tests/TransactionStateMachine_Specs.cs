namespace Sample.Components.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Contracts;
    using MassTransit;
    using NUnit.Framework;


    public class Receiving_a_dispatch_inbound_transaction_request :
        TransactionStateMachineTestFixture
    {
        [Test]
        public async Task Should_support_the_status_check()
        {
            var transactionId = NewId.NextGuid().ToString("N");
            var receiveTimestamp = DateTime.UtcNow;

            await TestHarness.Bus.Publish(new DispatchInboundRequest
            {
                TransactionId = transactionId,
                ReceiveTimestamp = receiveTimestamp,
                RoutingKey = "ABC"
            }, s => s.TimeToLive = TimeSpan.FromSeconds(5));

            var consumed = await TestHarness.Consumed.Any<DispatchInboundRequest>(x => x.Context.Message.TransactionId == transactionId);
            Assert.IsTrue(consumed, "DispatchInboundRequest not consumed");

            IList<Guid> correlationIds = await SagaHarness.Exists(state => state.TransactionId == transactionId, Machine.DispatchingRequest);
            Assert.That(correlationIds, Is.Not.Null.Or.Empty);
        }
    }
}
namespace Sample.Contracts
{
    using System;
    using MassTransit.Topology;


    [ExcludeFromTopology]
    public record RequestEvent
    {
        /// <summary>
        /// Unique transactionId, to identify this request and match up to subsequent response
        /// </summary>
        public string TransactionId { get; init; }

        /// <summary>
        /// The routing key/bin for the request
        /// </summary>
        public string RoutingKey { get; init; }

        /// <summary>
        /// Timestamp, in UTC, when the request was received
        /// </summary>
        public DateTime ReceiveTimestamp { get; init; }

        /// <summary>
        /// The incoming request messageId
        /// </summary>
        public Guid? RequestMessageId { get; set; }

        public DateTime? Deadline { get; set; }
    }
}
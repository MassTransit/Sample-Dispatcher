namespace Sample.Contracts
{
    using System;


    public record DispatchRequest
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
        /// The request body
        /// </summary>
        public string Body { get; init; }

        /// <summary>
        /// Timestamp, in UTC, when the request was received
        /// </summary>
        public DateTime ReceiveTimestamp { get; init; }
    }
}
namespace Sample.Contracts
{
    using System;


    public record DispatchResponse
    {
        /// <summary>
        /// Unique transactionId, to identify this request and match up to subsequent response
        /// </summary>
        public string? TransactionId { get; init; }

        /// <summary>
        /// The request body
        /// </summary>
        public string? Body { get; init; }

        /// <summary>
        /// Only present when request was available
        /// </summary>
        public string? RequestBody { get; init; }

        /// <summary>
        /// Timestamp, in UTC, when the response was received
        /// </summary>
        public DateTime? ResponseTimestamp { get; init; }
    }
}

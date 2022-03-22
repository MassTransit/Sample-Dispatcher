namespace Sample.Contracts
{
    using System;
    using MassTransit;


    [ExcludeFromTopology]
    public record ResponseEvent
    {
        /// <summary>
        /// Unique transactionId, to identify this request and match up to subsequent response
        /// </summary>
        public string TransactionId { get; init; } = null!;

        public DateTime? Deadline { get; init; }
    }
}

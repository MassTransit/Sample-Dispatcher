namespace Sample.Components.StateMachines
{
    using System;
    using MassTransit;


    public class TransactionState :
        SagaStateMachineInstance
    {
        public int CurrentState { get; set; }

        public string TransactionId { get; set; } = null!;

        public DateTime Created { get; set; }

        public DateTime? RequestReceived { get; set; }
        public DateTime? Deadline { get; set; }

        public DateTime? RequestCompleted { get; set; }
        public Uri? ResponseAddress { get; set; }

        public string? RequestBody { get; set; }
        public string? ResponseBody { get; set; }

        public Guid CorrelationId { get; set; }
    }
}

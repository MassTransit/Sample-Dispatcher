namespace Sample.Components.StateMachines
{
    using System;
    using Automatonymous;


    public class TransactionState :
        SagaStateMachineInstance
    {
        public int CurrentState { get; set; }

        public string TransactionId { get; set; } = null!;

        public DateTime Created { get; set; }
        public DateTime Received { get; set; }
        public DateTime? Deadline { get; set; }

        public Guid CorrelationId { get; set; }
    }
}
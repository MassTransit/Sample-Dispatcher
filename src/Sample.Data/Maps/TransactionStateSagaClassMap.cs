namespace Sample.Data.Maps
{
    using Components.StateMachines;
    using MassTransit.EntityFrameworkCoreIntegration.Mappings;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;


    public class TransactionStateSagaClassMap :
        SagaClassMap<TransactionState>
    {
        protected override void Configure(EntityTypeBuilder<TransactionState> entity, ModelBuilder model)
        {
            entity.Property(x => x.TransactionId).HasMaxLength(40).IsRequired();
            entity.HasIndex(x => x.TransactionId).IsUnique();

            entity.Property(x => x.CurrentState);

            entity.Property(x => x.Created);

            entity.Property(x => x.RequestReceived);
            entity.Property(x => x.Deadline);

            entity.Property(x => x.RequestCompleted);
            entity.Property(x => x.ResponseAddress).HasMaxLength(128);

            entity.Property(x => x.RequestBody);
            entity.Property(x => x.ResponseBody);
        }
    }
}

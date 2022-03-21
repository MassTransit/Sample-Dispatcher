namespace Sample.Data
{
    using System.Collections.Generic;
    using Maps;
    using MassTransit.EntityFrameworkCoreIntegration;
    using Microsoft.EntityFrameworkCore;


    public class SampleDbContext :
        SagaDbContext
    {
        public SampleDbContext(DbContextOptions<SampleDbContext> options)
            : base(options)
        {
        }

        protected override IEnumerable<ISagaClassMap> Configurations
        {
            get { yield return new TransactionStateSagaClassMap(); }
        }
    }
}

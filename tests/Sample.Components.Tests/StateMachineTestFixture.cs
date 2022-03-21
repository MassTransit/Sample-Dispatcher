namespace Sample.Components.Tests
{
    using System;
    using System.Threading.Tasks;
    using Internals;
    using MassTransit;
    using MassTransit.Testing;
    using Microsoft.Extensions.DependencyInjection;
    using NUnit.Framework;
    using NUnit.Framework.Internal;


    public class StateMachineTestFixture<TStateMachine, TInstance, TDefinition>
        where TStateMachine : class, SagaStateMachine<TInstance>
        where TInstance : class, SagaStateMachineInstance
        where TDefinition : class, ISagaDefinition<TInstance>
    {
        TestExecutionContext _fixtureContext;
        TestOutputLoggerFactory _loggerFactory;
        protected TStateMachine Machine;
        protected ServiceProvider Provider;
        protected ISagaStateMachineTestHarness<TStateMachine, TInstance> SagaHarness;
        protected ITestHarness TestHarness;

        [OneTimeSetUp]
        public async Task StateMachineTestFixtureOneTimeSetup()
        {
            _loggerFactory = new TestOutputLoggerFactory(true);

            var collection = new ServiceCollection();

            collection.AddMassTransitTestHarness(x =>
            {
                x.SetKebabCaseEndpointNameFormatter();

                x.AddDelayedMessageScheduler();

                x.AddSagaStateMachine<TStateMachine, TInstance, TDefinition>()
                    .InMemoryRepository();

                ConfigureMassTransit(x);

                x.UsingInMemory((context, cfg) =>
                {
                    cfg.UseDelayedMessageScheduler();

                    cfg.ConfigureEndpoints(context);
                });
            });

            ConfigureServices(collection);

            Provider = collection.BuildServiceProvider(true);

            TestHarness = Provider.GetTestHarness();
            TestHarness.TestInactivityTimeout = TimeSpan.FromSeconds(0.2);

            _fixtureContext = TestExecutionContext.CurrentContext;

            _loggerFactory.Current = _fixtureContext;

            await TestHarness.Start();

            SagaHarness = TestHarness.GetSagaStateMachineHarness<TStateMachine, TInstance>();
            Machine = SagaHarness.StateMachine;
        }

        protected virtual void ConfigureMassTransit(IBusRegistrationConfigurator configurator)
        {
        }

        protected virtual void ConfigureServices(IServiceCollection collection)
        {
        }

        [OneTimeTearDown]
        public async Task StateMachineTestFixtureOneTimeTearDown()
        {
            await Provider.DisposeAsync();
        }
    }
}

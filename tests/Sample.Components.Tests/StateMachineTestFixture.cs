namespace Sample.Components.Tests
{
    using System;
    using System.Threading.Tasks;
    using Automatonymous;
    using Internals;
    using MassTransit;
    using MassTransit.Context;
    using MassTransit.Definition;
    using MassTransit.ExtensionsDependencyInjectionIntegration;
    using MassTransit.Testing;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.DependencyInjection.Extensions;
    using Microsoft.Extensions.Logging;
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
        protected IStateMachineSagaTestHarness<TInstance, TStateMachine> SagaHarness;
        protected InMemoryTestHarness TestHarness;

        [OneTimeSetUp]
        public async Task StateMachineTestFixtureOneTimeSetup()
        {
            _loggerFactory = new TestOutputLoggerFactory(true);

            var collection = new ServiceCollection();

            collection.AddSingleton<ILoggerFactory>(_ => _loggerFactory);
            collection.TryAdd(ServiceDescriptor.Singleton(typeof(ILogger<>), typeof(Logger<>)));
            collection.AddMassTransitInMemoryTestHarness(cfg =>
            {
                cfg.SetKebabCaseEndpointNameFormatter();

                cfg.AddDelayedMessageScheduler();

                cfg.AddSagaStateMachine<TStateMachine, TInstance, TDefinition>()
                    .InMemoryRepository();

                cfg.AddSagaStateMachineTestHarness<TStateMachine, TInstance>();

                ConfigureMassTransit(cfg);
            });

            ConfigureServices(collection);

            Provider = collection.BuildServiceProvider(true);

            ConfigureLogging();

            TestHarness = Provider.GetRequiredService<InMemoryTestHarness>();
            TestHarness.TestInactivityTimeout = TimeSpan.FromSeconds(0.2);
            TestHarness.OnConfigureInMemoryBus += configurator =>
            {
                configurator.UseDelayedMessageScheduler();
            };

            _fixtureContext = TestExecutionContext.CurrentContext;

            _loggerFactory.Current = _fixtureContext;

            await TestHarness.Start();

            SagaHarness = Provider.GetRequiredService<IStateMachineSagaTestHarness<TInstance, TStateMachine>>();
            Machine = Provider.GetRequiredService<TStateMachine>();
        }

        protected virtual void ConfigureMassTransit(IServiceCollectionBusConfigurator configurator)
        {
        }

        protected virtual void ConfigureServices(IServiceCollection collection)
        {
        }

        [OneTimeTearDown]
        public async Task StateMachineTestFixtureOneTimeTearDown()
        {
            _loggerFactory.Current = _fixtureContext;

            try
            {
                await TestHarness.Stop();
            }
            finally
            {
                await Provider.DisposeAsync();
            }
        }

        void ConfigureLogging()
        {
            var loggerFactory = Provider.GetRequiredService<ILoggerFactory>();

            LogContext.ConfigureCurrentLogContext(loggerFactory);
        }
    }
}
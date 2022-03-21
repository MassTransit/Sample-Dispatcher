namespace Sample.Components.Tests
{
    using System.Reflection;
    using Configuration;
    using MassTransit.Internals;
    using MassTransit.Util;
    using Microsoft.Extensions.DependencyInjection;
    using Services;


    public static class ContainerConfigurationExtensions
    {
        public static void AddRequestRoutingCandidates(this IServiceCollection collection)
        {
            var types = AssemblyTypeCache.FindTypes(new[] { typeof(IRequestRoutingCandidate).Assembly }, type => type.HasInterface<IRequestRoutingCandidate>())
                .GetAwaiter().GetResult();

            foreach (var type in types.FindTypes(TypeClassification.Concrete | TypeClassification.Closed))
                collection.AddSingleton(typeof(IRequestRoutingCandidate), type);
        }

        public static void AddReceiveEndpointOptions(this IServiceCollection collection)
        {
            var types = AssemblyTypeCache.FindTypes(new[] { typeof(ReceiveEndpointOptions).Assembly }, x => x.BaseType == typeof(ReceiveEndpointOptions))
                .GetAwaiter().GetResult();

            foreach (var type in types.FindTypes(TypeClassification.Concrete | TypeClassification.Closed))
            {
                typeof(ContainerConfigurationExtensions)
                    .GetMethod(nameof(AddOptionsInternal), BindingFlags.Static | BindingFlags.NonPublic)
                    .MakeGenericMethod(type)
                    .Invoke(null, new object[] { collection });
            }
        }

        static void AddOptionsInternal<TOptions>(IServiceCollection collection)
            where TOptions : class
        {
            collection.AddOptions<TOptions>();
        }
    }
}

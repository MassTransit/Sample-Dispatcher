namespace Sample.Service
{
    using System.Reflection;
    using Components.Configuration;
    using Components.Services;
    using MassTransit.Internals.Extensions;
    using MassTransit.Util;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;


    public static class ContainerConfigurationExtensions
    {
        public static void AddRequestRoutingCandidates(this IServiceCollection collection)
        {
            var types = AssemblyTypeCache.FindTypes(new[] { typeof(IRequestRoutingCandidate).Assembly }, type => type.HasInterface<IRequestRoutingCandidate>())
                .GetAwaiter().GetResult();

            foreach (var type in types.FindTypes(TypeClassification.Concrete | TypeClassification.Closed))
                collection.AddSingleton(typeof(IRequestRoutingCandidate), type);
        }

        public static void AddReceiveEndpointOptions(this IServiceCollection collection, IConfiguration configuration)
        {
            var types = AssemblyTypeCache.FindTypes(new[] { typeof(ReceiveEndpointOptions).Assembly }, x => x.BaseType == typeof(ReceiveEndpointOptions))
                .GetAwaiter().GetResult();

            foreach (var type in types.FindTypes(TypeClassification.Concrete | TypeClassification.Closed))
            {
                typeof(ContainerConfigurationExtensions)
                    .GetMethod(nameof(AddOptionsInternal), BindingFlags.Static | BindingFlags.NonPublic)
                    .MakeGenericMethod(type)
                    .Invoke(null, new object[] { collection, configuration });
            }
        }

        static void AddOptionsInternal<TOptions>(IServiceCollection collection, IConfiguration configuration)
            where TOptions : class
        {
            var sectionName = typeof(TOptions).Name.Replace("Options", "");

            collection.Configure<TOptions>(configuration.GetSection($"Endpoint:{sectionName}"));
        }
    }
}

namespace Sample.Service
{
    using Components.Services;
    using MassTransit.Internals.Extensions;
    using MassTransit.Util;
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
    }
}
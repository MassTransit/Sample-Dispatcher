namespace Sample.Components.Consumers.TransGlobal
{
    using System;
    using System.Threading.Tasks;
    using MassTransit;
    using Services;


    public class TransGlobalRoutingCandidate :
        IRequestRoutingCandidate
    {
        readonly Uri _destinationAddress;

        public TransGlobalRoutingCandidate(IEndpointNameFormatter formatter)
        {
            _destinationAddress = new Uri($"exchange:{formatter.Consumer<TransGlobalRequestConsumer>()}");
        }

        public Task<RouteResult?> IsValidCandidate(RequestRoutingCriteria criteria)
        {
            return string.Equals(criteria.RoutingKey, "TransGlobal", StringComparison.OrdinalIgnoreCase)
                ? Task.FromResult<RouteResult?>(new RouteResult
                {
                    Disposition = RouteDisposition.Destination,
                    DestinationAddress = _destinationAddress
                })
                : Task.FromResult<RouteResult?>(null);
        }
    }
}

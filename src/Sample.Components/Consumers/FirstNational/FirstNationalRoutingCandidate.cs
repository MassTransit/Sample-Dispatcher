namespace Sample.Components.Consumers.FirstNational
{
    using System;
    using System.Threading.Tasks;
    using MassTransit;
    using Services;


    public class FirstNationalRoutingCandidate :
        IRequestRoutingCandidate
    {
        readonly Uri _destinationAddress;

        public FirstNationalRoutingCandidate(IEndpointNameFormatter formatter)
        {
            _destinationAddress = new Uri($"exchange:{formatter.Consumer<FirstNationalRequestConsumer>()}");
        }

        public Task<RouteResult?> IsValidCandidate(RequestRoutingCriteria criteria)
        {
            return string.Equals(criteria.RoutingKey, "FirstNatl", StringComparison.OrdinalIgnoreCase)
                ? Task.FromResult<RouteResult?>(new RouteResult
                {
                    Disposition = RouteDisposition.Destination,
                    DestinationAddress = _destinationAddress
                })
                : Task.FromResult<RouteResult?>(null);
        }
    }
}

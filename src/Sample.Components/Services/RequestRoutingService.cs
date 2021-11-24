namespace Sample.Components.Services
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Contracts;


    public class RequestRoutingService :
        IRequestRoutingService
    {
        readonly List<IRequestRoutingCandidate> _candidates;

        public RequestRoutingService(IEnumerable<IRequestRoutingCandidate> candidates)
        {
            _candidates = candidates.ToList();
        }

        public async Task<RouteResult> RouteRequest(DispatchInboundRequest request)
        {
            var criteria = new RequestRoutingCriteria(request.RoutingKey);

            List<RouteResult> routeResults = (await Task.WhenAll(_candidates.Select(candidate => candidate.IsValidCandidate(criteria))))
                .Where(x => x != null)
                .Select(x => x!)
                .ToList();

            if (routeResults.Count == 0)
                return new RouteResult { Disposition = RouteDisposition.Unhandled };

            if (routeResults.Count > 1)
                return new RouteResult { Disposition = RouteDisposition.Ambiguous };

            return routeResults.Single();
        }
    }
}
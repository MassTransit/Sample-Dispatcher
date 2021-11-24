namespace Sample.Components.Services
{
    public readonly struct RequestRoutingCriteria
    {
        public readonly string RoutingKey;

        public RequestRoutingCriteria(string routingKey)
        {
            RoutingKey = routingKey;
        }
    }
}
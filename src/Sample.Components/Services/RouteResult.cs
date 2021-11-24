namespace Sample.Components.Services
{
    using System;


    public record RouteResult
    {
        public RouteDisposition Disposition { get; init; }

        public Uri? DestinationAddress { get; init; }
    }
}

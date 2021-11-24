namespace Sample.Components
{
    using System;


    public interface IServiceEndpointLocator
    {
        public Uri TransactionStateEndpointAddress { get; }

        Uri DispatchRequestEndpointAddress { get; }
    }
}
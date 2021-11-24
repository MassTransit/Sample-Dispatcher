namespace Sample.Api.Models
{
    using System.ComponentModel.DataAnnotations;


    public record DispatchRequestModel
    {
        [Required]
        public string TransactionId { get; init; }

        [Required]
        public string RoutingKey { get; init; }

        [Required]
        public string Body { get; init; }
    }
}
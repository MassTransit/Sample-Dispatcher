namespace Sample.Api.Models
{
    using System.ComponentModel.DataAnnotations;


    public record DispatchResponseModel
    {
        [Required]
        public string TransactionId { get; init; }

        [Required]
        public string Body { get; init; }
    }
}
